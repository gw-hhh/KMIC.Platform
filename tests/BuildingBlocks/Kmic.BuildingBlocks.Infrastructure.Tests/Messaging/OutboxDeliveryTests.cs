using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

/// <summary>
/// TicketChanged 路由到 Echo 模块，TicketClosedNotice 没有订阅。各测试按自己的消息 ID 断言
/// </summary>
public sealed class OutboxDeliveryTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private const string Source = "BuildingBlocks";
    private const string Mirror = "Echo.MirrorTicket";
    private const string Audit = "Echo.AuditTicket";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private OutboxDispatcher Dispatcher => _harness.Services.GetRequiredService<OutboxDispatcher>();

    public async ValueTask InitializeAsync() =>
        await new MessageRoutes(database.DataSource).RegisterAsync(
            [new RouteDeclaration(new MessageRoute("BuildingBlocks.TicketChanged", Mirror, "Echo"), false),
             new RouteDeclaration(new MessageRoute("BuildingBlocks.TicketChanged", Audit, "Echo"), false)],
            [],
            "test",
            Ct);

    [Fact]
    public async Task Deliver_CopiesToEverySubscription_ThenMarksDelivered()
    {
        var messageId = await ShareAsync();

        await DrainAsync();

        var outbox = await OutboxAsync(messageId);
        outbox.Status.ShouldBe(MessageStatus.Done);
        outbox.DeliveredAt.ShouldNotBeNull();
        var inbox = await InboxAsync(messageId);
        inbox.Select(r => r.Subscription).ShouldBe([Audit, Mirror], ignoreOrder: true);
        inbox.ShouldAllBe(r => r.SourceModule == Source && r.MessageType == "BuildingBlocks.TicketChanged" && r.SourceVersion == 1 && r.Seq == outbox.Seq);
        inbox.ShouldAllBe(r => r.Status == MessageStatus.Pending);
    }

    [Fact]
    public async Task NoSubscriber_MarkedDelivered()
    {
        var ticket = await NewTicketAsync();
        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new CloseTicketCommand(ticket.Id), Ct)).ShouldBeSuccess();
        }

        await DrainAsync();

        await using var check = _harness.BeginAsSystem();
        var notices = await check.Db.Set<OutboxMessage>().AsNoTracking().Where(m => m.MessageType == "BuildingBlocks.TicketClosedNotice").ToListAsync(Ct);
        notices.ShouldNotBeEmpty();
        notices.ShouldAllBe(m => m.Status == MessageStatus.Done);
    }

    [Fact]
    public async Task ConcurrentClaims_NeverShareAMessage()
    {
        for (var i = 0; i < 6; i++)
        {
            await ShareAsync();
        }

        var claims = await Task.WhenAll(
            Dispatcher.ClaimAsync(Source, 4, Guid.CreateVersion7(), Ct),
            Dispatcher.ClaimAsync(Source, 4, Guid.CreateVersion7(), Ct));

        claims[0].Select(m => m.Id).Intersect(claims[1].Select(m => m.Id)).ShouldBeEmpty();
        (claims[0].Count + claims[1].Count).ShouldBeGreaterThanOrEqualTo(6);
    }

    [Fact]
    public async Task LeaseExpired_OriginalDelivererCannotWriteOrMark()
    {
        var messageId = await ShareAsync();
        var first = Guid.CreateVersion7();
        var claimedByFirst = (await Dispatcher.ClaimAsync(Source, 100, first, Ct)).Single(m => m.Id == messageId);
        await ExpireLeaseAsync(messageId);

        var second = Guid.CreateVersion7();
        var claimedBySecond = (await Dispatcher.ClaimAsync(Source, 100, second, Ct)).Single(m => m.Id == messageId);
        var routes = await new MessageRoutes(database.DataSource).ListDeliveringAsync(Ct);

        (await Dispatcher.DeliverAsync(Source, claimedByFirst, first, routes, Ct)).ShouldBe(DeliveryResult.TakenOver);
        (await Dispatcher.DeliverAsync(Source, claimedBySecond, second, routes, Ct)).ShouldBe(DeliveryResult.Delivered);
        (await InboxAsync(messageId)).Count.ShouldBe(2);
    }

    [Fact]
    public async Task LateDelivererAfterCleanup_WritesNothing()
    {
        var messageId = await ShareAsync();
        var late = Guid.CreateVersion7();
        var claimedByLate = (await Dispatcher.ClaimAsync(Source, 100, late, Ct)).Single(m => m.Id == messageId);
        await ExpireLeaseAsync(messageId);
        await DrainAsync();

        // 按保留规则清理：先删来源行，再删 Inbox 里的去重凭证
        await ExecuteAsync("""DELETE FROM "Probe"."OutboxMessage" WHERE "Id" = @id""", messageId);
        await ExecuteAsync("""DELETE FROM "Echo"."InboxMessage" WHERE "MessageId" = @id""", messageId);

        var routes = await new MessageRoutes(database.DataSource).ListDeliveringAsync(Ct);
        (await Dispatcher.DeliverAsync(Source, claimedByLate, late, routes, Ct)).ShouldBe(DeliveryResult.TakenOver);
        (await InboxAsync(messageId)).ShouldBeEmpty();
    }

    [Fact]
    public async Task RedeliveredAfterLeaseExpiry_InboxNotDuplicated()
    {
        var messageId = await ShareAsync();
        var token = Guid.CreateVersion7();
        var claimed = (await Dispatcher.ClaimAsync(Source, 100, token, Ct)).Single(m => m.Id == messageId);
        var routes = await new MessageRoutes(database.DataSource).ListDeliveringAsync(Ct);

        (await Dispatcher.DeliverAsync(Source, claimed, token, routes, Ct)).ShouldBe(DeliveryResult.Delivered);

        // 模拟写完 Inbox、标记已投递之前进程停了：占用到期后别的投递者重新投递
        await ExecuteAsync("""UPDATE "Probe"."OutboxMessage" SET "Status" = 1, "LeaseUntil" = clock_timestamp() - interval '1 second' WHERE "Id" = @id""", messageId);
        await DrainAsync();

        (await InboxAsync(messageId)).Count.ShouldBe(2);
        (await OutboxAsync(messageId)).Status.ShouldBe(MessageStatus.Done);
    }

    public async ValueTask DisposeAsync() => await _harness.DisposeAsync();

    private Task<Ticket> NewTicketAsync() => _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));

    private async Task<Guid> ShareAsync()
    {
        var ticket = await NewTicketAsync();
        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, ["已分享"]), Ct)).ShouldBeSuccess();
        }

        await using var check = _harness.BeginAsSystem();
        return await check.Db.Set<OutboxMessage>().Where(m => m.SourceId == ticket.Id.Value).Select(m => m.Id).SingleAsync(Ct);
    }

    private async Task DrainAsync()
    {
        while (await Dispatcher.DeliverBatchAsync(Source, Ct) > 0)
        {
        }
    }

    private async Task<OutboxMessage> OutboxAsync(Guid id)
    {
        await using var check = _harness.BeginAsSystem();
        return await check.Db.Set<OutboxMessage>().AsNoTracking().SingleAsync(m => m.Id == id, Ct);
    }

    private async Task<List<InboxMessage>> InboxAsync(Guid messageId)
    {
        await using var command = database.DataSource.CreateCommand(
            """SELECT "Subscription", "MessageType", "SourceModule", "SourceVersion", "Seq", "Status" FROM "Echo"."InboxMessage" WHERE "MessageId" = @id""");
        command.Parameters.AddWithValue("id", messageId);
        await using var reader = await command.ExecuteReaderAsync(Ct);
        var rows = new List<InboxMessage>();
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(new InboxMessage
            {
                MessageId = messageId,
                Subscription = reader.GetString(0),
                MessageType = reader.GetString(1),
                SourceModule = reader.GetString(2),
                SourceVersion = await reader.GetFieldValueAsync<long?>(3, Ct),
                Seq = reader.GetInt64(4),
                Status = reader.GetInt16(5),
            });
        }

        return rows;
    }

    private Task ExpireLeaseAsync(Guid id) =>
        ExecuteAsync("""UPDATE "Probe"."OutboxMessage" SET "LeaseUntil" = clock_timestamp() - interval '1 second' WHERE "Id" = @id""", id);

    private async Task ExecuteAsync(string sql, Guid id)
    {
        await using var command = database.DataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync(Ct);
    }
}

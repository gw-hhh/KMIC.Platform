using System.Text.Json;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
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
/// Probe 发布单据快照，Echo 有序处理。每个测试用自己的单据，顺序键互不影响；
/// 处理执行了几次，看 Echo 的 Outbox 里有几条 TicketMirrored
/// </summary>
public sealed class InboxProcessingTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private const string Unknown = "Echo.NotInThisBuild";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private InboxProcessor Processor => _harness.Services.GetRequiredService<InboxProcessor>();

    public async ValueTask InitializeAsync()
    {
        var catalog = _harness.Services.GetRequiredService<SubscriptionCatalog>();

        // 另登记一个本实例代码里没有的订阅，模拟新版本才有的订阅
        await new MessageRoutes(database.DataSource).RegisterAsync(
            [.. MessageRoutes.Declared(catalog), new RouteDeclaration(new MessageRoute("BuildingBlocks.TicketChanged", Unknown, "Echo"), false)],
            [],
            "test",
            Ct);
    }

    [Fact]
    public async Task Processed_RunsAsSystemOnBehalfOfInitiator_InSameTransaction()
    {
        var (ticket, messageId) = await PublishAsync("同步过去");

        await DrainAsync();

        (await InboxAsync(messageId, ProbeHarness.MirrorSubscription)).Status.ShouldBe(MessageStatus.Done);
        var mirrored = (await MirroredAsync(ticket)).ShouldHaveSingleItem();
        JsonDocument.Parse(mirrored.Payload).RootElement.GetProperty("title").GetString().ShouldBe("同步过去");
        var headers = JsonDocument.Parse(mirrored.Headers).RootElement;
        headers.GetProperty("causationId").GetGuid().ShouldBe(messageId);
        headers.GetProperty("initiatorUserId").GetGuid().ShouldBe(_alice.UserId.Value);
        headers.GetProperty("sourceModule").GetString().ShouldBe("Echo");
    }

    [Fact]
    public async Task UnknownSubscription_LeftForNewerBuild()
    {
        var (_, messageId) = await PublishAsync("新订阅的");

        await DrainAsync();

        (await InboxAsync(messageId, Unknown)).Status.ShouldBe(MessageStatus.Pending);
        (await InboxAsync(messageId, Unknown)).Attempts.ShouldBe(0);
    }

    [Fact]
    public async Task BusinessFailure_DeadAtOnce_NothingCommitted()
    {
        _harness.Echo.Failures["会被拒绝"] = Error.Conflict("Echo.Rejected", "不接受");
        var (ticket, messageId) = await PublishAsync("会被拒绝");

        await DrainAsync();

        var row = await InboxAsync(messageId, ProbeHarness.MirrorSubscription);
        row.Status.ShouldBe(MessageStatus.Dead);
        row.Attempts.ShouldBe(1);
        row.LastError.ShouldNotBeNull().ShouldContain("Echo.Rejected");
        (await MirroredAsync(ticket)).ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NotReadyOrException_RetriedThenProcessed(bool throws)
    {
        if (throws)
        {
            _harness.Echo.Throws = 1;
        }
        else
        {
            _harness.Echo.Failures["稍后再来"] = CommonErrors.NotReadyYet;
        }

        var (ticket, messageId) = await PublishAsync("稍后再来");
        await DrainAsync();

        var afterFirst = await InboxAsync(messageId, ProbeHarness.MirrorSubscription);
        afterFirst.Status.ShouldBe(MessageStatus.Pending);
        afterFirst.LastError.ShouldNotBeNull().ShouldContain(throws ? "模拟处理器出错" : CommonErrors.NotReadyYet.Code);

        await MakeDueAsync(messageId);
        await DrainAsync();

        (await InboxAsync(messageId, ProbeHarness.MirrorSubscription)).Status.ShouldBe(MessageStatus.Done);
        (await MirroredAsync(ticket)).ShouldHaveSingleItem();
    }

    [Fact]
    public async Task TakenOverWhileRunning_OriginalCommitRejected()
    {
        var (ticket, messageId) = await PublishAsync("处理中被接管");
        await DeliverAsync();
        _harness.Echo.BeforeReturn = async ct =>
        {
            await using var takeOver = database.DataSource.CreateCommand(
                """UPDATE "Echo"."InboxMessage" SET "LeaseToken" = @other WHERE "MessageId" = @id""");
            takeOver.Parameters.AddWithValue("other", Guid.CreateVersion7());
            takeOver.Parameters.AddWithValue("id", messageId);
            await takeOver.ExecuteNonQueryAsync(ct);
        };

        var token = Guid.CreateVersion7();
        var claimed = (await Processor.ClaimAsync("Echo", 100, token, Ct)).Single(m => m.MessageId == messageId);
        (await Processor.ProcessAsync("Echo", claimed, token, Ct)).ShouldBe(InboxResult.TakenOver);

        (await MirroredAsync(ticket)).ShouldBeEmpty();
        (await InboxAsync(messageId, ProbeHarness.MirrorSubscription)).Status.ShouldBe(MessageStatus.Processing);
    }

    [Fact]
    public async Task ConfirmationLostAfterCommit_RecordedAsProcessedNotRerun()
    {
        var (ticket, messageId) = await PublishAsync("确认丢失");
        await DeliverAsync();
        var token = Guid.CreateVersion7();
        var claimed = (await Processor.ClaimAsync("Echo", 100, token, Ct)).Single(m => m.MessageId == messageId);
        (await Processor.ProcessAsync("Echo", claimed, token, Ct)).ShouldBe(InboxResult.Processed);

        // 提交成功了，处理程序却以为失败：带条件的记录影响 0 行，重新读出来是已处理
        (await Processor.RecordFailureAsync("Echo", claimed, token, "连接断开", dead: false, null, Ct)).ShouldBe(InboxResult.Processed);

        (await InboxAsync(messageId, ProbeHarness.MirrorSubscription)).Status.ShouldBe(MessageStatus.Done);
        (await MirroredAsync(ticket)).ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Ordered_DeadMessageBlocksSameKeyOnly()
    {
        _harness.Echo.Failures["第一次"] = Error.Conflict("Echo.Rejected", "不接受");
        var ticket = await NewTicketAsync();
        var first = await ShareAsync(ticket, "第一次");
        var second = await ShareAsync(ticket, "第二次");
        var (other, _) = await PublishAsync("别的单据");

        await DrainAsync();

        (await InboxAsync(first, ProbeHarness.MirrorSubscription)).Status.ShouldBe(MessageStatus.Dead);
        (await InboxAsync(second, ProbeHarness.MirrorSubscription)).Status.ShouldBe(MessageStatus.Pending);
        (await MirroredAsync(other)).ShouldHaveSingleItem();

        // 人工处理掉死信之后，同一个键后面的消息接着处理
        await ExecuteAsync("""UPDATE "Echo"."InboxMessage" SET "Status" = 4 WHERE "MessageId" = @id""", first);
        await DrainAsync();

        (await InboxAsync(second, ProbeHarness.MirrorSubscription)).Status.ShouldBe(MessageStatus.Done);
    }

    [Fact]
    public async Task UnreadablePayload_DeadAtOnce()
    {
        var (_, messageId) = await PublishAsync("内容坏了");
        await DeliverAsync();
        await ExecuteAsync("""UPDATE "Echo"."InboxMessage" SET "Payload" = '{"unexpected":1}' WHERE "MessageId" = @id""", messageId);

        await DrainAsync();

        var row = await InboxAsync(messageId, ProbeHarness.MirrorSubscription);
        row.Status.ShouldBe(MessageStatus.Dead);
        row.LastError.ShouldNotBeNull().ShouldContain("读不出来");
    }

    public async ValueTask DisposeAsync() => await _harness.DisposeAsync();

    private Task<Ticket> NewTicketAsync() => _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));

    private async Task<(Ticket Ticket, Guid MessageId)> PublishAsync(string title)
    {
        var ticket = await NewTicketAsync();
        return (ticket, await ShareAsync(ticket, title));
    }

    private async Task<Guid> ShareAsync(Ticket ticket, string title)
    {
        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, [title]), Ct)).ShouldBeSuccess();
        }

        await using var check = _harness.BeginAsSystem();
        return await check.Db.Set<OutboxMessage>().Where(m => m.SourceId == ticket.Id.Value).OrderByDescending(m => m.Seq).Select(m => m.Id).FirstAsync(Ct);
    }

    private async Task DeliverAsync()
    {
        var dispatcher = _harness.Services.GetRequiredService<OutboxDispatcher>();
        while (await dispatcher.DeliverBatchAsync("BuildingBlocks", Ct) > 0)
        {
        }
    }

    private async Task DrainAsync()
    {
        await DeliverAsync();
        while (await Processor.ProcessBatchAsync("Echo", Ct) > 0)
        {
        }
    }

    private async Task<InboxMessage> InboxAsync(Guid messageId, string subscription)
    {
        await using var command = database.DataSource.CreateCommand(
            """SELECT "Status", "Attempts", "LastError" FROM "Echo"."InboxMessage" WHERE "MessageId" = @id AND "Subscription" = @subscription""");
        command.Parameters.AddWithValue("id", messageId);
        command.Parameters.AddWithValue("subscription", subscription);
        await using var reader = await command.ExecuteReaderAsync(Ct);
        (await reader.ReadAsync(Ct)).ShouldBeTrue();
        return new InboxMessage
        {
            MessageId = messageId,
            Subscription = subscription,
            Status = reader.GetInt16(0),
            Attempts = reader.GetInt32(1),
            LastError = await reader.IsDBNullAsync(2, Ct) ? null : reader.GetString(2),
        };
    }

    private async Task<List<(string Payload, string Headers)>> MirroredAsync(Ticket ticket)
    {
        await using var command = database.DataSource.CreateCommand(
            """SELECT "Payload"::text, "Headers"::text FROM "Echo"."OutboxMessage" WHERE "MessageType" = 'Echo.TicketMirrored' AND "Payload"->>'ticketId' = @id""");
        command.Parameters.AddWithValue("id", ticket.Id.Value.ToString());
        await using var reader = await command.ExecuteReaderAsync(Ct);
        var rows = new List<(string, string)>();
        while (await reader.ReadAsync(Ct))
        {
            rows.Add((reader.GetString(0), reader.GetString(1)));
        }

        return rows;
    }

    private Task MakeDueAsync(Guid messageId) =>
        ExecuteAsync("""UPDATE "Echo"."InboxMessage" SET "NextAttemptAt" = clock_timestamp() WHERE "MessageId" = @id""", messageId);

    private async Task ExecuteAsync(string sql, Guid id)
    {
        await using var command = database.DataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync(Ct);
    }
}

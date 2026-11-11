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
/// TicketClosedNotice 路由到一个本实例没有注册的模块，投递一定失败
/// </summary>
public sealed class OutboxDeliveryFailureTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString, messaging: o => o.MaxAttempts = 2);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() =>
        await new MessageRoutes(database.DataSource).RegisterAsync(
            [new RouteDeclaration(new MessageRoute("BuildingBlocks.TicketClosedNotice", "Ghost.Watch", "Ghost"), false)], [], "test", Ct);

    [Fact]
    public async Task UnknownSubscriberModule_RetriedThenDead()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));
        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new CloseTicketCommand(ticket.Id), Ct)).ShouldBeSuccess();
        }

        var dispatcher = _harness.Services.GetRequiredService<OutboxDispatcher>();
        await dispatcher.DeliverBatchAsync("BuildingBlocks", Ct);

        var afterFirst = await NoticeAsync();
        afterFirst.Status.ShouldBe(MessageStatus.Pending);
        afterFirst.Attempts.ShouldBe(1);
        afterFirst.LastError.ShouldNotBeNull().ShouldContain("Ghost");
        await using (var later = database.DataSource.CreateCommand("""SELECT "NextAttemptAt" > clock_timestamp() FROM "Probe"."OutboxMessage" WHERE "Id" = @id"""))
        {
            later.Parameters.AddWithValue("id", afterFirst.Id);
            (await later.ExecuteScalarAsync(Ct)).ShouldBe(true);
        }

        // 不等退避，直接让它到期
        await using (var due = database.DataSource.CreateCommand("""UPDATE "Probe"."OutboxMessage" SET "NextAttemptAt" = clock_timestamp() WHERE "Id" = @id"""))
        {
            due.Parameters.AddWithValue("id", afterFirst.Id);
            await due.ExecuteNonQueryAsync(Ct);
        }

        await dispatcher.DeliverBatchAsync("BuildingBlocks", Ct);

        var afterSecond = await NoticeAsync();
        afterSecond.Status.ShouldBe(MessageStatus.Dead);
        afterSecond.Attempts.ShouldBe(2);
    }

    public async ValueTask DisposeAsync() => await _harness.DisposeAsync();

    private async Task<OutboxMessage> NoticeAsync()
    {
        await using var check = _harness.BeginAsSystem();
        return await check.Db.Set<OutboxMessage>().AsNoTracking().SingleAsync(m => m.MessageType == "BuildingBlocks.TicketClosedNotice", Ct);
    }
}

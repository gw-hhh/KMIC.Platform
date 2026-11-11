using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

public sealed class MessagingWorkerTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Running_PublishedSnapshotReachesSubscriberWithoutPolling()
    {
        // 兜底轮询设成很长，能及时处理只能是靠通知唤醒
        await using var harness = new ProbeHarness(database.ConnectionString, messaging: o => o.PollInterval = TimeSpan.FromMinutes(5));
        var worker = harness.Services.GetServices<IHostedService>().ShouldHaveSingleItem();
        await worker.StartAsync(Ct);
        try
        {
            var ticket = await harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));

            // 等后台登记好路由再发布；通知要是早于监听连上，监听连上时补查的那一次也会处理它
            await Eventually.UntilAsync(
                async ct =>
                {
                    await using var command = database.DataSource.CreateCommand(
                        """SELECT count(*) FROM "Operations"."MessageRoute" WHERE "Subscription" = @name""");
                    command.Parameters.AddWithValue("name", ProbeHarness.MirrorSubscription);
                    return (long)(await command.ExecuteScalarAsync(ct))! == 1;
                },
                "后台登记路由",
                TimeSpan.FromSeconds(15),
                Ct);
            await using (var work = harness.Begin(_alice))
            {
                (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, ["后台送达"]), Ct)).ShouldBeSuccess();
            }

            await Eventually.UntilAsync(
                async ct =>
                {
                    await using var command = database.DataSource.CreateCommand(
                        """SELECT count(*) FROM "Echo"."OutboxMessage" WHERE "MessageType" = 'Echo.TicketMirrored' AND "Payload"->>'ticketId' = @id""");
                    command.Parameters.AddWithValue("id", ticket.Id.Value.ToString());
                    return (long)(await command.ExecuteScalarAsync(ct))! == 1;
                },
                "Echo 处理完单据快照",
                TimeSpan.FromSeconds(15),
                Ct);
        }
        finally
        {
            await worker.StopAsync(Ct);
        }
    }
}

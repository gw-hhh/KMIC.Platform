using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Messaging;
using Kmic.Echo.Probe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

/// <summary>
/// 模块测试的标准写法：发布方命令 → DrainAsync → 断言订阅方的结果
/// </summary>
public sealed class MessagingHarnessTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Drain_RunsDeliveryAndHandlingAcrossModules()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));
        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, ["跨模块"]), Ct)).ShouldBeSuccess();
        }

        await _harness.Services.DrainAsync(Ct);

        await using (var check = _harness.BeginAsSystem())
        {
            var summary = await check.Services.GetRequiredService<EchoQueryDbContext>().Tickets.SingleAsync(t => t.Id == ticket.Id.Value, Ct);
            summary.Title.ShouldBe("跨模块");
            summary.SourceVersion.ShouldBe(1);
        }

        var changed = (await _harness.Services.PublishedAsync<TicketChanged>(Ct)).Single(m => m.Message.Ticket.Id == ticket.Id.Value);
        changed.Envelope.SourceVersion.ShouldBe(1);
        changed.Envelope.InitiatorUserId.ShouldBe(_alice.UserId.Value);
        var mirrored = (await _harness.Services.PublishedAsync<TicketMirrored>(Ct)).ShouldHaveSingleItem();
        mirrored.CausationId.ShouldBe(changed.Envelope.MessageId);
        mirrored.Envelope.SourceModule.ShouldBe("Echo");
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();
}

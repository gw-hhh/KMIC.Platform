using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Pipeline;

/// <summary>
/// 命令改了数据再查本模块会抛异常，先查后改不受影响
/// </summary>
public sealed class QueryVisibilityGuardTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task QueryAfterModification_Throws()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"], "原标题"));
        await using var work = _harness.Begin(_alice);

        var error = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await work.Sender.SendAsync(new RetitleAndReadCommand(ticket.Id, "新标题", ReadFirst: false), Ct));
        error.Message.ShouldContain("读不到命令里还没保存的修改");
    }

    [Fact]
    public async Task QueryBeforeModification_Allowed()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"], "原标题"));
        await using var work = _harness.Begin(_alice);

        (await work.Sender.SendAsync(new RetitleAndReadCommand(ticket.Id, "新标题", ReadFirst: true), Ct)).ShouldBeSuccess().ShouldBe("原标题");
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();
}

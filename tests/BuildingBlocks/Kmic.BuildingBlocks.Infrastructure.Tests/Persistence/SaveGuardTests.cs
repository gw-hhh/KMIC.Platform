using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 只有命令事务能保存；查询上下文不跟踪，也不能保存
/// </summary>
public sealed class SaveGuardTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task SaveChangesAsync_OutsideTransactionBehavior_Throws()
    {
        await using var work = _harness.Begin(_alice);
        work.Db.Add(NewTicket());

        var error = await Should.ThrowAsync<InvalidOperationException>(() => work.Db.SaveChangesAsync(Ct));
        error.Message.ShouldContain("处理器不能自己保存");
    }

    [Fact]
    public async Task SaveChanges_Synchronous_NotSupported()
    {
        await using var work = _harness.Begin(_alice);
        work.Db.Add(NewTicket());

        Should.Throw<NotSupportedException>(() => work.Db.SaveChanges());
    }

    [Fact]
    public async Task QueryContext_DoesNotTrackAndCannotSave()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());
        await using var work = _harness.Begin(_alice);
        var context = (ProbeQueryDbContext)work.Query;

        var loaded = await work.Query.Tickets.SingleAsync(t => t.Id == ticket.Id, Ct);
        loaded.Rename("查询上下文里的修改");

        context.ChangeTracker.Entries().ShouldBeEmpty();
        await Should.ThrowAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static Ticket NewTicket() => Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]);
}

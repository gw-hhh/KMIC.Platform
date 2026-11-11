using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 聚合内任何实体变化都让聚合根版本加一，UPDATE 条件带原版本
/// </summary>
public sealed class ConcurrencyVersionTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Add_StartsAtZero()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());

        (await VersionOfAsync(ticket.Id)).ShouldBe(0);
    }

    [Fact]
    public async Task EveryKindOfChange_IncrementsRootVersionOnce()
    {
        var ticket = NewTicket();
        var kept = ticket.AddLine("保留");
        var removed = ticket.AddLine("移除");
        await _harness.AddAsync(_alice, ticket);

        await EditAsync(ticket.Id, t => t.Rename("只改根"));
        (await VersionOfAsync(ticket.Id)).ShouldBe(1);

        await EditAsync(ticket.Id, t => t.ChangeLine(kept.Id, "只改子实体"));
        (await VersionOfAsync(ticket.Id)).ShouldBe(2);

        await EditAsync(ticket.Id, t => t.AddLine("只加子实体"));
        (await VersionOfAsync(ticket.Id)).ShouldBe(3);

        await EditAsync(ticket.Id, t => t.RemoveLine(removed.Id));
        (await VersionOfAsync(ticket.Id)).ShouldBe(4);

        await EditAsync(ticket.Id, t =>
        {
            t.Rename("一次保存里改多处");
            t.ChangeLine(kept.Id, "同时改子实体");
        });
        (await VersionOfAsync(ticket.Id)).ShouldBe(5);
    }

    [Fact]
    public async Task InterleavedSaves_SecondFails()
    {
        var ticket = NewTicket();
        var line = ticket.AddLine("原文");
        await _harness.AddAsync(_alice, ticket);

        await using var first = _harness.Begin(_alice);
        await using var second = _harness.Begin(_alice);
        var mine = await LoadAsync(first, ticket.Id);
        var theirs = await LoadAsync(second, ticket.Id);

        mine.Rename("先保存");
        await first.SaveAsync();

        // 后保存的一方只改了子实体，同样被版本拦下
        theirs.ChangeLine(line.Id, "后保存");
        await Should.ThrowAsync<DbUpdateConcurrencyException>(second.SaveAsync);
    }

    [Fact]
    public async Task ChildChangedWithoutRoot_Throws()
    {
        var ticket = NewTicket();
        var line = ticket.AddLine("原文");
        await _harness.AddAsync(_alice, ticket);

        await using var work = _harness.Begin(_alice);
        var detached = await work.Db.Set<TicketLine>().SingleAsync(l => l.Id == line.Id, Ct);
        detached.Change("绕过聚合根");

        var error = await Should.ThrowAsync<InvalidOperationException>(work.SaveAsync);
        error.Message.ShouldContain("聚合根没有被跟踪");
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static Ticket NewTicket() => Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]);

    private static Task<Ticket> LoadAsync(ProbeWork work, TicketId id) =>
        work.Db.Set<Ticket>().Include(t => t.Lines).SingleAsync(t => t.Id == id, Ct);

    private async Task EditAsync(TicketId id, Action<Ticket> edit)
    {
        await using var work = _harness.Begin(_alice);
        edit(await LoadAsync(work, id));
        await work.SaveAsync();
    }

    private async Task<long> VersionOfAsync(TicketId id)
    {
        await using var work = _harness.BeginAsSystem();
        return await work.Query.Tickets.Where(t => t.Id == id).Select(t => t.ConcurrencyVersion).SingleAsync(Ct);
    }
}

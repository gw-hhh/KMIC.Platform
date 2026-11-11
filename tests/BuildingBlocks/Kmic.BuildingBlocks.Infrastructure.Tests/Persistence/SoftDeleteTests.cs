using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 聚合根的删除改写为标记删除，子实体随之保留；唯一约束只约束未删除的行
/// </summary>
public sealed class SoftDeleteTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Remove_MarksDeletedAndKeepsChildren()
    {
        var ticket = Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]);
        ticket.AddLine("保留的行");
        await _harness.AddAsync(_alice, ticket);
        _harness.Time.Advance(TimeSpan.FromHours(1));

        await using (var work = _harness.Begin(_alice))
        {
            work.Db.Remove(await work.Db.Set<Ticket>().Include(t => t.Lines).SingleAsync(t => t.Id == ticket.Id, Ct));
            await work.SaveAsync();
        }

        await using var check = _harness.BeginAsSystem();
        var row = await check.Db.Set<Ticket>()
            .IgnoreQueryFilters([QueryFilters.SoftDelete])
            .Where(t => t.Id == ticket.Id)
            .Select(t => new
            {
                IsDeleted = EF.Property<bool>(t, SoftDeleteColumns.IsDeleted),
                DeletedAt = EF.Property<DateTimeOffset?>(t, SoftDeleteColumns.DeletedAt),
                DeletedBy = EF.Property<UserId?>(t, SoftDeleteColumns.DeletedBy),
                Lines = t.Lines.Count,
            })
            .SingleAsync(Ct);
        row.IsDeleted.ShouldBeTrue();
        row.DeletedAt.ShouldBe(_harness.Time.GetUtcNow());
        row.DeletedBy.ShouldBe(_alice.UserId);
        row.Lines.ShouldBe(1);
    }

    [Fact]
    public async Task Removed_HiddenFromDefaultQueries_VisibleInRecycleBinWithinDataScope()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));
        await RemoveAsync(ticket.Id);

        await using var work = _harness.Begin(_alice);
        (await work.Query.Tickets.AnyAsync(t => t.Id == ticket.Id, Ct)).ShouldBeFalse();
        (await work.Query.Tickets.IgnoreQueryFilters([QueryFilters.SoftDelete]).AnyAsync(t => t.Id == ticket.Id, Ct)).ShouldBeTrue();

        // 回收站只关闭软删除，看不到别人的数据
        await using var stranger = _harness.Begin(TestIdentity.User("丙").InTree(_org).InOrgUnit(_org["总部"]));
        (await stranger.Query.Tickets.IgnoreQueryFilters([QueryFilters.SoftDelete]).AnyAsync(t => t.Id == ticket.Id, Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task Add_SameNumberAsDeleted_Allowed_SameNumberAsLive_Rejected()
    {
        var number = TicketNumbers.Next();
        var first = await _harness.AddAsync(_alice, Ticket.Create(number, _alice.UserId, _org["总部"]));
        await RemoveAsync(first.Id);

        await _harness.AddAsync(_alice, Ticket.Create(number, _alice.UserId, _org["总部"]));

        await Should.ThrowAsync<DbUpdateException>(() => _harness.AddAsync(_alice, Ticket.Create(number, _alice.UserId, _org["总部"])));
    }

    [Fact]
    public async Task RemoveLine_DeletesChildRowPhysically()
    {
        var ticket = Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]);
        var line = ticket.AddLine("要移除的行");
        await _harness.AddAsync(_alice, ticket);

        await using (var work = _harness.Begin(_alice))
        {
            (await work.Db.Set<Ticket>().Include(t => t.Lines).SingleAsync(t => t.Id == ticket.Id, Ct)).RemoveLine(line.Id);
            await work.SaveAsync();
        }

        await using var check = _harness.BeginAsSystem();
        (await check.Db.Set<TicketLine>().AnyAsync(l => l.Id == line.Id, Ct)).ShouldBeFalse();
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private async Task RemoveAsync(TicketId id)
    {
        await using var work = _harness.Begin(_alice);
        work.Db.Remove(await work.Db.Set<Ticket>().SingleAsync(t => t.Id == id, Ct));
        await work.SaveAsync();
    }
}

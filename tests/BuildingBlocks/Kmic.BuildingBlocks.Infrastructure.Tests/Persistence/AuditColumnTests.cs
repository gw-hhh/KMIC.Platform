using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 审计列：时间取 TimeProvider，操作人取执行上下文
/// </summary>
public sealed class AuditColumnTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);
    private static readonly TestIdentity _bob = TestIdentity.User("乙").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Add_WritesCreatedColumnsOnly()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());

        var audit = await AuditOfAsync(ticket.Id);

        audit.CreatedAt.ShouldBe(_harness.Time.GetUtcNow());
        audit.CreatedBy.ShouldBe(_alice.UserId);
        audit.UpdatedAt.ShouldBeNull();
        audit.UpdatedBy.ShouldBeNull();
    }

    [Fact]
    public async Task Modify_WritesUpdatedColumnsAndKeepsCreated()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());
        _harness.Time.Advance(TimeSpan.FromMinutes(5));

        await using (var work = _harness.Begin(_bob.WithRole(r => r.Scope(ScopeLevel.All))))
        {
            (await work.Db.Set<Ticket>().SingleAsync(t => t.Id == ticket.Id, Ct)).Rename("改名");
            await work.SaveAsync();
        }

        var audit = await AuditOfAsync(ticket.Id);
        audit.CreatedBy.ShouldBe(_alice.UserId);
        audit.UpdatedAt.ShouldBe(_harness.Time.GetUtcNow());
        audit.UpdatedBy.ShouldBe(_bob.UserId);
    }

    [Fact]
    public async Task Add_AsSystem_WritesSystemUser()
    {
        var ticket = NewTicket();
        await using (var work = _harness.BeginAsSystem())
        {
            work.Db.Add(ticket);
            await work.SaveAsync();
        }

        (await AuditOfAsync(ticket.Id)).CreatedBy.ShouldBe(UserId.System);
    }

    [Fact]
    public async Task Add_WhileImpersonating_WritesActualUser()
    {
        var admin = UserId.New();
        var ticket = await _harness.AddAsync(_alice.ImpersonatedBy(admin, allowWrite: true), NewTicket());

        (await AuditOfAsync(ticket.Id)).CreatedBy.ShouldBe(admin);
    }

    [Fact]
    public async Task Add_Anonymous_WritesSystemUser()
    {
        // 登录、找回密码这类匿名请求写的数据，记在系统用户名下
        var ticket = NewTicket();
        await using (var work = _harness.Begin(CallOrigin.Http, CurrentUser.Anonymous))
        {
            work.Db.Add(ticket);
            await work.SaveAsync();
        }

        (await AuditOfAsync(ticket.Id)).CreatedBy.ShouldBe(UserId.System);
    }

    [Fact]
    public async Task AddChild_ChildGetsCreatedColumnsAndRootGetsUpdated()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());
        _harness.Time.Advance(TimeSpan.FromMinutes(1));

        TicketLineId lineId;
        await using (var work = _harness.Begin(_alice))
        {
            var loaded = await work.Db.Set<Ticket>().Include(t => t.Lines).SingleAsync(t => t.Id == ticket.Id, Ct);
            lineId = loaded.AddLine("第一行").Id;
            await work.SaveAsync();
        }

        await using var check = _harness.BeginAsSystem();
        var line = await check.Db.Set<TicketLine>()
            .Where(l => l.Id == lineId)
            .Select(l => new { CreatedAt = EF.Property<DateTimeOffset>(l, AuditColumns.CreatedAt), CreatedBy = EF.Property<UserId>(l, AuditColumns.CreatedBy) })
            .SingleAsync(Ct);
        line.CreatedAt.ShouldBe(_harness.Time.GetUtcNow());
        line.CreatedBy.ShouldBe(_alice.UserId);
        (await AuditOfAsync(ticket.Id)).UpdatedAt.ShouldBe(_harness.Time.GetUtcNow());
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static Ticket NewTicket() => Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]);

    private async Task<AuditRow> AuditOfAsync(TicketId id)
    {
        await using var work = _harness.BeginAsSystem();
        return await work.Db.Set<Ticket>()
            .Where(t => t.Id == id)
            .Select(t => new AuditRow(
                EF.Property<DateTimeOffset>(t, AuditColumns.CreatedAt),
                EF.Property<UserId>(t, AuditColumns.CreatedBy),
                EF.Property<DateTimeOffset?>(t, AuditColumns.UpdatedAt),
                EF.Property<UserId?>(t, AuditColumns.UpdatedBy)))
            .SingleAsync(Ct);
    }

    private sealed record AuditRow(DateTimeOffset CreatedAt, UserId CreatedBy, DateTimeOffset? UpdatedAt, UserId? UpdatedBy);
}

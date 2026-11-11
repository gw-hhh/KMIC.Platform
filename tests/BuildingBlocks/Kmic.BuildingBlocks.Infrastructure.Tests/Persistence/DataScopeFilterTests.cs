using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 每个测试开始时在上海、杭州、广州各建一张单据，所有者分别是销售甲、乙、丙。同一个类的测试共用数据库，断言按标题去重
/// </summary>
public sealed class DataScopeFilterTests(ClassDatabase<ProbeTemplate> database)
    : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部", hq => hq
        .Unit("华东", east => east.Unit("上海").Unit("杭州"))
        .Unit("华南", south => south.Unit("广州"))));

    private static readonly TestIdentity _shanghai = TestIdentity.User("销售甲").InTree(_org).InOrgUnit(_org["上海"]);
    private static readonly TestIdentity _hangzhou = TestIdentity.User("销售乙").InTree(_org).InOrgUnit(_org["杭州"]);
    private static readonly TestIdentity _guangzhou = TestIdentity.User("销售丙").InTree(_org).InOrgUnit(_org["广州"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);
    private readonly Dictionary<string, TicketId> _tickets = [];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        foreach (var (city, owner) in new[] { ("上海", _shanghai), ("杭州", _hangzhou), ("广州", _guangzhou) })
        {
            var ticket = await _harness.AddAsync(owner, Ticket.Create(TicketNumbers.Next(), owner.UserId, _org[city], title: city));
            _tickets[city] = ticket.Id;
        }
    }

    [Fact]
    public async Task Self_SeesOwnTicketsOnly()
    {
        (await VisibleToAsync(_shanghai.WithRole(r => r.Scope(ScopeLevel.Self)))).ShouldBe(["上海"]);
    }

    [Fact]
    public async Task OrgUnitAndBelowFromAnchor_SeesSubtreeAndOwn()
    {
        var manager = TestIdentity.User("华东经理").InTree(_org).InOrgUnit(_org["总部"])
            .WithRole(r => r.Scope(ScopeLevel.OrgUnitAndBelow))
            .WithAnchors(_org["华东"]);

        (await VisibleToAsync(manager)).ShouldBe(["上海", "杭州"], ignoreOrder: true);
    }

    [Fact]
    public async Task All_SeesEverything()
    {
        (await VisibleToAsync(_guangzhou.WithRole(r => r.Scope(ScopeLevel.All)))).ShouldBe(["上海", "杭州", "广州"], ignoreOrder: true);
    }

    [Fact]
    public async Task SystemIdentity_SeesEverything()
    {
        await using var work = _harness.BeginAsSystem();

        (await TitlesAsync(work.Query.Tickets)).ShouldBe(["上海", "杭州", "广州"], ignoreOrder: true);
    }

    [Fact]
    public async Task Anonymous_SeesNothing()
    {
        await using var work = _harness.Begin(CallOrigin.Http, CurrentUser.Anonymous);

        (await TitlesAsync(work.Query.Tickets)).ShouldBeEmpty();
    }

    [Fact]
    public async Task WriteContext_AlsoFiltered()
    {
        // 仓储通过写入上下文读取，范围外的数据同样读不到
        await using var work = _harness.Begin(_shanghai);

        (await work.Db.Set<Ticket>().AnyAsync(t => t.Id == _tickets["广州"], Ct)).ShouldBeFalse();
        (await work.Db.Set<Ticket>().AnyAsync(t => t.Id == _tickets["上海"], Ct)).ShouldBeTrue();
    }

    [Fact]
    public async Task WithoutDataScope_KeepsSoftDeleteFilter()
    {
        var deleted = await _harness.AddAsync(_shanghai, Ticket.Create(TicketNumbers.Next(), _shanghai.UserId, _org["上海"], title: "已删除"));
        await using (var remove = _harness.Begin(_shanghai))
        {
            remove.Db.Remove(await remove.Db.Set<Ticket>().SingleAsync(t => t.Id == deleted.Id, Ct));
            await remove.SaveAsync();
        }

        await using var work = _harness.Begin(_guangzhou);
        var titles = await TitlesAsync(work.Query.Tickets.WithoutDataScope());

        titles.ShouldBe(["上海", "杭州", "广州"], ignoreOrder: true);
    }

    [Fact]
    public async Task Sql_ScopeValuesAreParameters()
    {
        var manager = _shanghai.WithRole(r => r.Scope(ScopeLevel.OrgUnitAndBelow)).WithAnchors(_org["华东"]);
        await using var work = _harness.Begin(manager);

        // 开头的注释行里有参数值，只检查语句本身
        var sql = string.Join('\n', work.Query.Tickets.Select(t => t.Title).ToQueryString()
            .Split('\n')
            .Where(line => !line.StartsWith("--", StringComparison.Ordinal)));

        sql.ShouldContain("= ANY (");
        sql.ShouldNotContain(_org["上海"].Value.ToString());
        sql.ShouldNotContain(manager.UserId.Value.ToString());
    }

    public async ValueTask DisposeAsync() => await _harness.DisposeAsync();

    private static Task<List<string>> TitlesAsync(IQueryable<Ticket> tickets) => tickets.Select(t => t.Title).Distinct().ToListAsync(Ct);

    private async Task<List<string>> VisibleToAsync(TestIdentity identity)
    {
        await using var work = _harness.Begin(identity);
        return await TitlesAsync(work.Query.Tickets);
    }
}

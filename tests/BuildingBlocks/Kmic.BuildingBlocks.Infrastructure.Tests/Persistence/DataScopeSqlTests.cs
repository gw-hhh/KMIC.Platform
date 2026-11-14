using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 手写 SQL 的数据范围条件：在每种范围下，查到的行必须和 EF 查询过滤器查到的完全一样。
/// 每个测试开始时在上海、杭州、广州各建一张单据，所有者分别是销售甲、乙、丙，断言按标题去重
/// </summary>
public sealed class DataScopeSqlTests(ClassDatabase<ProbeTemplate> database)
    : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部", hq => hq
        .Unit("华东", east => east.Unit("上海").Unit("杭州"))
        .Unit("华南", south => south.Unit("广州"))));

    private static readonly TestIdentity _shanghai = TestIdentity.User("销售甲").InTree(_org).InOrgUnit(_org["上海"]);
    private static readonly TestIdentity _hangzhou = TestIdentity.User("销售乙").InTree(_org).InOrgUnit(_org["杭州"]);
    private static readonly TestIdentity _guangzhou = TestIdentity.User("销售丙").InTree(_org).InOrgUnit(_org["广州"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        foreach (var (city, owner) in new[] { ("上海", _shanghai), ("杭州", _hangzhou), ("广州", _guangzhou) })
        {
            await _harness.AddAsync(owner, Ticket.Create(TicketNumbers.Next(), owner.UserId, _org[city], title: city));
        }
    }

    public static TheoryData<string> Scenarios =>
        ["self", "orgUnit", "orgUnitAndBelowFromAnchor", "all", "system", "anonymous"];

    [Theory]
    [MemberData(nameof(Scenarios))]
    public async Task TheSqlConditionFindsTheSameRowsAsTheQueryFilter(string scenario)
    {
        await using var work = Begin(scenario);

        var byFilter = await work.Query.Tickets.Select(t => t.Title).Distinct().ToListAsync(Ct);
        var bySql = await TitlesBySqlAsync(work, "t.\"OwnerId\"", "t.\"OrgUnitId\"");

        bySql.ShouldBe(byFilter, ignoreOrder: true);
        // 场景本身的预期也要对，不能只是两边相等
        bySql.ShouldBe(Expected(scenario), ignoreOrder: true);
    }

    [Fact]
    public async Task ATableWithOnlyAnOwnerColumnOrOnlyAnOrgColumn_UsesJustThatColumn()
    {
        await using var work = Begin("orgUnit");

        var ownerOnly = await TitlesBySqlAsync(work, "t.\"OwnerId\"", orgUnitColumn: null);
        var orgOnly = await TitlesBySqlAsync(work, ownerColumn: null, "t.\"OrgUnitId\"");
        var neither = await TitlesBySqlAsync(work, null, null);

        // 同事不是所有者：只看所有者列什么也看不到，只看组织列才能看到上海的单据
        ownerOnly.ShouldBeEmpty();
        orgOnly.ShouldBe(["上海"]);
        neither.ShouldBeEmpty("既没有所有者列也没有组织列，没有任何可见范围");
    }

    [Fact]
    public async Task TheScopeValuesAreParameters_NotPartOfTheSqlText()
    {
        await using var work = Begin("orgUnitAndBelowFromAnchor");

        var condition = work.Services.GetRequiredService<IDataScopeSql>().Condition(ProbeScopes.Ticket, "t.\"OwnerId\"", "t.\"OrgUnitId\"");

        condition.Format.ShouldBe("(t.\"OwnerId\" = {0} OR t.\"OrgUnitId\" = ANY({1}))");
        condition.ArgumentCount.ShouldBe(2);
        condition.Format.ShouldNotContain(_shanghai.UserId.Value.ToString());
    }

    [Theory]
    [InlineData("t.\"OwnerId\"; DROP TABLE x")]
    [InlineData("t.\"OwnerId\" OR 1=1")]
    [InlineData("\"a\".\"b\".\"c\"")]
    [InlineData("t.OwnerId--")]
    [InlineData("")]
    [InlineData("t.\"Own\"erId\"")]
    public async Task AColumnThatIsNotAnIdentifier_IsRefused(string column)
    {
        await using var work = Begin("self");
        var sql = work.Services.GetRequiredService<IDataScopeSql>();

        Should.Throw<ArgumentException>(() => sql.Condition(ProbeScopes.Ticket, column, null));
        Should.Throw<ArgumentException>(() => sql.Condition(ProbeScopes.Ticket, null, column));
    }

    [Theory]
    [InlineData("t.OwnerId")]
    [InlineData("OwnerId")]
    [InlineData("\"OwnerId\"")]
    [InlineData("\"t\".\"OwnerId\"")]
    public async Task PlainAndQuotedColumnsAreAccepted(string column)
    {
        await using var work = Begin("self");

        work.Services.GetRequiredService<IDataScopeSql>().Condition(ProbeScopes.Ticket, column, null).Format.ShouldContain(column);
    }

    [Fact]
    public void Concat_RenumbersThePlaceholdersAndKeepsTheArguments_AndEscapedBraces()
    {
        var id = Guid.CreateVersion7();
        FormattableString first = $"SELECT '{{x}}' WHERE a = {id} AND ";
        FormattableString second = $"b = ANY({new[] { 1, 2 }}) AND c = {"文本"} ORDER BY 1";

        var joined = SqlFragments.Concat(first, second);

        joined.Format.ShouldBe("SELECT '{{x}}' WHERE a = {0} AND b = ANY({1}) AND c = {2} ORDER BY 1");
        var arguments = joined.GetArguments();
        arguments.Length.ShouldBe(3);
        arguments[0].ShouldBe(id);
        ((int[])arguments[1]!).ShouldBe([1, 2]);
        arguments[2].ShouldBe("文本");
        SqlFragments.Concat().Format.ShouldBeEmpty();
    }

    public async ValueTask DisposeAsync() => await _harness.DisposeAsync();

    private static string[] Expected(string scenario) => scenario switch
    {
        "self" => ["上海"],
        "orgUnit" => ["上海"],
        "orgUnitAndBelowFromAnchor" => ["上海", "杭州"],
        "all" or "system" => ["上海", "杭州", "广州"],
        _ => [],
    };

    private ProbeWork Begin(string scenario) => scenario switch
    {
        "self" => _harness.Begin(_shanghai.WithRole(r => r.Scope(ScopeLevel.Self))),
        // 上海的同事：不是任何一张单据的所有者，只能靠所属组织看到上海的单据
        "orgUnit" => _harness.Begin(TestIdentity.User("上海同事").InTree(_org).InOrgUnit(_org["上海"]).WithRole(r => r.Scope(ScopeLevel.OrgUnit))),
        "orgUnitAndBelowFromAnchor" => _harness.Begin(
            TestIdentity.User("华东经理").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Scope(ScopeLevel.OrgUnitAndBelow)).WithAnchors(_org["华东"])),
        "all" => _harness.Begin(_guangzhou.WithRole(r => r.Scope(ScopeLevel.All))),
        "system" => _harness.BeginAsSystem(),
        "anonymous" => _harness.Begin(CallOrigin.Http, CurrentUser.Anonymous),
        _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
    };

    private static async Task<List<string>> TitlesBySqlAsync(ProbeWork work, string? ownerColumn, string? orgUnitColumn)
    {
        var condition = work.Services.GetRequiredService<IDataScopeSql>().Condition(ProbeScopes.Ticket, ownerColumn, orgUnitColumn);
        var query = SqlFragments.Concat(
            $"SELECT DISTINCT t.\"Title\" AS \"Value\" FROM \"Probe\".\"Ticket\" t WHERE t.\"IsDeleted\" = false AND ",
            condition);
        return await work.Db.Database.SqlQuery<string>(query).ToListAsync(Ct);
    }
}

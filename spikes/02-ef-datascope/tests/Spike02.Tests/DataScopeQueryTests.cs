using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shouldly;
using Spike02.Domain;
using Spike02.Infrastructure;
using Xunit;

namespace Spike02.Tests;

// 在 PG 18 上实际执行：返回的行、数组参数的类型转换、查询只编译一次
public sealed class DataScopeQueryTests(PostgresFixture db) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task Owner_org_unit_and_relation_branches_return_expected_rows()
    {
        await using var context = db.CreateContext(FakeDataScope.User(Ids.Me, Ids.East));

        var visible = await VisibleAsync(context);

        // S6 有协作关系但为私有且不是本人的：资源可见性优先（05-05 第 5.8 节）
        visible.ShouldBe(["S1 我的、华东", "S2 他人、华东", "S5 他人、华南、我是协作人"], ignoreOrder: true);
    }

    [Fact]
    public async Task Multiple_org_units_bind_as_one_array_parameter()
    {
        await using var context = db.CreateContext(FakeDataScope.User(Ids.Me, Ids.West, Ids.North));

        var visible = await VisibleAsync(context);

        visible.ShouldBe(["S1 我的、华东", "S3 他人、华西", "S5 他人、华南、我是协作人"], ignoreOrder: true);
    }

    [Fact]
    public async Task Empty_org_unit_list_leaves_owner_and_relation()
    {
        await using var context = db.CreateContext(FakeDataScope.User(Ids.Me));

        var visible = await VisibleAsync(context);

        visible.ShouldBe(["S1 我的、华东", "S5 他人、华南、我是协作人"], ignoreOrder: true);
    }

    [Fact]
    public async Task System_identity_sees_everything_except_deleted()
    {
        await using var context = db.CreateContext(FakeDataScope.System());

        var visible = await VisibleAsync(context);

        visible.Count.ShouldBe(6);
        visible.ShouldNotContain(v => string.Equals(v, "S7 我的、已删除", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Ignoring_data_scope_keeps_soft_delete()
    {
        await using var context = db.CreateContext(FakeDataScope.User(Ids.Me));

        var visible = await VisibleAsync(context, q => q.IgnoreQueryFilters([ModuleDbContext.DataScopeFilter]));

        visible.Count.ShouldBe(6);
        visible.ShouldNotContain(v => string.Equals(v, "S7 我的、已删除", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Same_query_with_different_scopes_compiles_once()
    {
        // 三个上下文共用同一份选项（同一个内部服务提供者与查询缓存），与应用中的上下文池一致
        var compilations = 0;
        var options = db.CreateOptions(builder =>
            builder.LogTo(_ => Interlocked.Increment(ref compilations), [CoreEventId.QueryCompilationStarting]));

        foreach (var scope in new[] { FakeDataScope.User(Ids.Me, Ids.East), FakeDataScope.User(Ids.Other, Ids.West, Ids.North), FakeDataScope.User(Ids.Me) })
        {
            await using var context = new SampleDbContext(options, scope);
            await context.Samples.Select(s => s.Id).ToListAsync(TestContext.Current.CancellationToken);
        }

        compilations.ShouldBe(1);
    }

    [Fact]
    public async Task Keyset_comparison_matches_dotnet_ordering()
    {
        await using var context = db.CreateContext(FakeDataScope.System());
        var all = db.Samples.Values.Where(id => id != db.Samples["S7 我的、已删除"]).OrderBy(id => id).ToList();
        var cursor = all[3];

        var page = await context.Samples.Where(s => s.Id.CompareTo(cursor) < 0).OrderBy(s => s.Id).Select(s => s.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        page.ShouldBe(all.Take(3));
    }

    private async Task<List<string>> VisibleAsync(SampleDbContext context, Func<IQueryable<Sample>, IQueryable<Sample>>? shape = null)
    {
        var ids = await (shape ?? (q => q))(context.Samples).Select(s => s.Id).ToListAsync(TestContext.Current.CancellationToken);
        return [.. db.Samples.Where(p => ids.Contains(p.Value)).Select(p => p.Key)];
    }
}

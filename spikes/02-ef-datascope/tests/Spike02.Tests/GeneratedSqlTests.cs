using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Spike02.Infrastructure;
using Xunit;

namespace Spike02.Tests;

// 不连接数据库：只检查 EF 生成的 SQL 与参数（ToQueryString）
public sealed partial class GeneratedSqlTests
{
    private static readonly DbContextOptions<SampleDbContext> _options =
        new DbContextOptionsBuilder<SampleDbContext>().UseNpgsql("Host=localhost;Database=spike02").Options;

    [Fact]
    public void Filter_values_come_from_the_executing_context()
    {
        // 第一个实例同时用于构建模型；第二个实例的范围不同，SQL 必须相同、参数必须取第二个实例的值
        using var first = new SampleDbContext(_options, FakeDataScope.User(Ids.Me, Ids.East));
        var secondScope = FakeDataScope.User(Ids.Other, Ids.West, Ids.North);
        using var second = new SampleDbContext(_options, secondScope);

        var firstSql = first.Samples.ToQueryString();
        var secondSql = second.Samples.ToQueryString();
        Log(firstSql);
        Log(secondSql);

        Body(secondSql).ShouldBe(Body(firstSql));
        var secondParameters = Parameters(secondSql);
        secondParameters.ShouldContain(Ids.Other.Value.ToString());
        secondParameters.ShouldContain(Ids.West.Value.ToString());
        secondParameters.ShouldContain(Ids.North.Value.ToString());
        secondParameters.ShouldNotContain(Ids.Me.Value.ToString());
        secondParameters.ShouldNotContain(Ids.East.Value.ToString());
        Log($"For calls per query: {secondScope.ForCalls}");
    }

    [Fact]
    public void Scope_values_are_parameters_not_literals()
    {
        using var context = new SampleDbContext(_options, FakeDataScope.User(Ids.Me, Ids.East, Ids.West));

        var body = Body(context.Samples.ToQueryString());

        GuidPattern().IsMatch(body).ShouldBeFalse(body);
        body.ShouldContain("= ANY (");
        body.ShouldContain("EXISTS (");
    }

    [Fact]
    public void Ignoring_data_scope_keeps_soft_delete()
    {
        using var context = new SampleDbContext(_options, FakeDataScope.User(Ids.Me, Ids.East));

        var body = Body(context.Samples.IgnoreQueryFilters([ModuleDbContext.DataScopeFilter]).ToQueryString());
        Log(body);

        body.ShouldContain("IsDeleted");
        body.ShouldNotContain("ANY (");
        body.ShouldNotContain("EXISTS (");
    }

    [Fact]
    public void System_identity_without_current_user_still_translates()
    {
        using var context = new SampleDbContext(_options, FakeDataScope.System());

        var sql = context.Samples.ToQueryString();
        Log(sql);

        sql.ShouldContain("IsDeleted");
    }

    [Fact]
    public void Keyset_comparison_on_strongly_typed_id_with_operator()
    {
        using var context = new SampleDbContext(_options, FakeDataScope.System());

        // 游标来自请求参数（局部变量）；static readonly 字段会被 EF 10 当作常量内联
        var cursor = Ids.Cursor;
        var sql = context.Samples.Where(s => s.Id < cursor).ToQueryString();
        Log(sql);

        Body(sql).ShouldContain("\"Id\" < @");
    }

    [Fact]
    public void Keyset_comparison_on_strongly_typed_id_with_compare_to()
    {
        using var context = new SampleDbContext(_options, FakeDataScope.System());

        var cursor = Ids.Cursor;
        var sql = context.Samples.Where(s => s.Id.CompareTo(cursor) < 0).ToQueryString();
        Log(sql);

        Body(sql).ShouldContain("\"Id\" < @");
    }

    private static string Body(string sql) =>
        string.Join('\n', sql.Split('\n').Where(line => !line.StartsWith("--", StringComparison.Ordinal) && !line.StartsWith("SET ", StringComparison.Ordinal)));

    private static string Parameters(string sql) =>
        string.Join('\n', sql.Split('\n').Where(line => line.StartsWith("--", StringComparison.Ordinal) || line.StartsWith("SET ", StringComparison.Ordinal)));

    private static void Log(string text) => TestContext.Current.TestOutputHelper?.WriteLine(text + "\n");

    [GeneratedRegex("[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", RegexOptions.IgnoreCase, 1000)]
    private static partial Regex GuidPattern();
}

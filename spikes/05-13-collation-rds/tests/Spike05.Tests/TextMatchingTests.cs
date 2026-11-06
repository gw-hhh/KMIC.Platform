using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shouldly;
using Xunit;

namespace Spike05.Tests;

// 原型 #5：ADR-0030 与 06 第 11.2 节的待确认项
public sealed class TextMatchingTests(TargetDatabase target) : IClassFixture<TargetDatabase>, IAsyncLifetime
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        if (target.TextSchemaCreated)
        {
            return;
        }

        target.TextSchemaCreated = true;
        await using var context = CreateContext();
        var script = context.Database.GenerateCreateScript();
        Log(script);
        await context.Database.ExecuteSqlRawAsync(script, Ct);
        context.Items.AddRange(
            new TextItem { Name = "华东客户 ACME", Code = "ABC-001" },
            new TextItem { Name = "华南客户 acme", Code = "abc-002" },
            new TextItem { Name = "折扣 50% 客户", Code = "X_Y" },
            new TextItem { Name = "折扣 500 客户", Code = "XzY" },
            new TextItem { Name = @"路径 C:\temp", Code = "Café" });
        await context.SaveChangesAsync(Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Case_insensitive_column_compares_without_case()
    {
        await using var context = CreateContext();

        (await context.Items.CountAsync(i => i.Code == "abc-001", Ct)).ShouldBe(1);
        (await context.Items.CountAsync(i => i.Name == "华东客户 acme", Ct)).ShouldBe(0);
    }

    [Fact]
    public async Task Pg18_like_on_a_nondeterministic_column()
    {
        // PG 18 起非确定性排序规则支持 LIKE；ILIKE 仍不支持
        var like = await Db.QueryAsync(target.Source, $"""SELECT "Code" FROM "{target.Schema}"."TextItem" WHERE "Code" LIKE '%abc%' ORDER BY 1""");
        Log("LIKE '%abc%': " + string.Join(", ", like.Select(r => r[0])));
        like.Count.ShouldBe(2);

        var ilike = await Should.ThrowAsync<PostgresException>(() =>
            Db.QueryAsync(target.Source, $"""SELECT "Code" FROM "{target.Schema}"."TextItem" WHERE "Code" ILIKE '%abc%'"""));
        Log("ILIKE: " + ilike.MessageText);
    }

    [Theory]
    [InlineData("acme", 2)]
    [InlineData("客户", 4)]
    [InlineData("50%", 1)]
    [InlineData(@"C:\t", 1)]
    [InlineData("华东", 1)]
    public async Task Contains_text_on_the_default_column(string term, int expected)
    {
        await using var context = CreateContext();
        var query = context.Items.Where(i => KmicFunctions.ContainsText(i.Name, term));
        Log(query.ToQueryString());

        (await query.CountAsync(Ct)).ShouldBe(expected);
    }

    [Theory]
    [InlineData("abc", 2)]
    [InlineData("ABC-00", 2)]
    [InlineData("x_y", 1)]
    [InlineData("café", 1)]
    public async Task Contains_text_on_the_case_insensitive_column(string term, int expected)
    {
        await using var context = CreateContext();

        (await context.Items.CountAsync(i => KmicFunctions.ContainsText(i.Code, term), Ct)).ShouldBe(expected);
    }

    [Fact]
    public async Task Starts_with_text_and_parameters()
    {
        await using var context = CreateContext();
        var term = "abc";
        var query = context.Items.Where(i => KmicFunctions.StartsWithText(i.Code, term));
        var sql = query.ToQueryString();
        Log(sql);

        sql.ShouldContain("COLLATE \"C\" ILIKE");
        sql.ShouldContain("@term");
        (await query.CountAsync(Ct)).ShouldBe(2);
    }

    private TextContext CreateContext() => new(target.ConnectionString, target.Schema);

    private static void Log(string text) => TestContext.Current.TestOutputHelper?.WriteLine(text + "\n");
}

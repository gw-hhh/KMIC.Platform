using System.Diagnostics.Metrics;
using Kmic.BuildingBlocks.Infrastructure.Observability;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Observability;

public sealed class SlowQueryLogTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private const string Secret = "hidden-parameter-value";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("SELECT 1", "SELECT")]
    [InlineData("  update \"T\" SET \"A\" = 1", "UPDATE")]
    [InlineData("-- tag: 列表\nSELECT t.\"Id\" FROM \"T\" AS t", "SELECT")]
    [InlineData("\n\n-- a\n-- b\nINSERT INTO \"T\" VALUES (1)", "INSERT")]
    [InlineData("WITH c AS (SELECT 1) DELETE FROM \"T\"", "WITH")]
    [InlineData("DELETE\tFROM \"T\"", "DELETE")]
    [InlineData("-- only a comment", "UNKNOWN")]
    public void StatementKind_SkipsCommentsAndTakesFirstWord(string sql, string expected) =>
        SlowQueryInterceptor.StatementKind(sql).ShouldBe(expected);

    [Fact]
    public async Task SlowQuery_LogsWarningWithParameterizedSqlAndCounts()
    {
        await using var harness = Harness(o => o.SlowQueryMs = 50);
        using var metrics = new MetricRecorder(harness.Services.GetRequiredService<IMeterFactory>());

        await RunSlowQueryAsync(harness);

        var log = Logs(harness).ShouldHaveSingleItem();
        log.Level.ShouldBe(LogLevel.Warning);
        log.Id.Id.ShouldBe(2801);
        log.Message.ShouldContain("BuildingBlocks");
        log.Message.ShouldContain("pg_sleep(");
        log.Message.ShouldNotContain(Secret);
        metrics.Of("kmic.db.slow_queries").ShouldHaveSingleItem().Tags["module"].ShouldBe("BuildingBlocks");
    }

    [Fact]
    public async Task FastQuery_NotLogged()
    {
        await using var harness = Harness(o => o.SlowQueryMs = 60_000);
        using var metrics = new MetricRecorder(harness.Services.GetRequiredService<IMeterFactory>());

        await RunSlowQueryAsync(harness);

        Logs(harness).ShouldBeEmpty();
        metrics.Of("kmic.db.slow_queries").ShouldBeEmpty();
    }

    [Fact]
    public async Task RecordSqlTextOff_LogsOnlyTheStatementKind()
    {
        await using var harness = Harness(o =>
        {
            o.SlowQueryMs = 50;
            o.RecordSqlText = false;
        });

        await RunSlowQueryAsync(harness);

        var message = Logs(harness).ShouldHaveSingleItem().Message;
        message.ShouldEndWith("SELECT");
        message.ShouldNotContain("pg_sleep");
    }

    [Fact]
    public async Task LongStatement_TruncatedTo4KB()
    {
        await using var harness = Harness(o => o.SlowQueryMs = 50);
        var columns = string.Join(", ", Enumerable.Range(0, 600).Select(i => $"1 AS column_{i}"));
        var sql = string.Concat("SELECT pg_sleep(0.1), ", columns);

        await using (var work = harness.Begin(TestIdentity.User("甲")))
        {
            var context = work.Services.GetRequiredService<ProbeQueryDbContext>();
            await context.Database.ExecuteSqlRawAsync(sql, Ct);
        }

        var message = Logs(harness).ShouldHaveSingleItem().Message;
        message.ShouldContain("…");
        message.Length.ShouldBeLessThan(4500);
    }

    [Fact]
    public async Task SlowFailingQuery_StillLoggedAndMarkedFailed()
    {
        await using var harness = Harness(o => o.SlowQueryMs = 50);

        await using (var work = harness.Begin(TestIdentity.User("甲")))
        {
            var context = work.Services.GetRequiredService<ProbeQueryDbContext>();
            // 除数在子查询里，到执行时才会算出 0：先睡 0.2 秒，再报错
            await Should.ThrowAsync<PostgresException>(
                () => context.Database.ExecuteSqlRawAsync("SELECT pg_sleep(0.2), 1 / (SELECT count(*)::int FROM pg_class WHERE false)", Ct));
        }

        Logs(harness).ShouldHaveSingleItem().Message.ShouldContain("failed True");
    }

    [Fact]
    public async Task WriteContextStatements_AlsoObserved()
    {
        await using var harness = Harness(o => o.SlowQueryMs = 50);

        await using (var work = harness.Begin(TestIdentity.User("甲")))
        {
            await work.Db.Database.ExecuteSqlRawAsync("SELECT pg_sleep(0.1)", Ct);
        }

        Logs(harness).ShouldHaveSingleItem().Message.ShouldContain("BuildingBlocks");
    }

    private static IReadOnlyList<FakeLogRecord> Logs(ProbeHarness harness) =>
        [.. harness.Services.GetFakeLogCollector().GetSnapshot().Where(r => r.Id.Id == 2801)];

    private static async Task RunSlowQueryAsync(ProbeHarness harness)
    {
        await using var work = harness.Begin(TestIdentity.User("甲"));
        var context = work.Services.GetRequiredService<ProbeQueryDbContext>();
        var rows = await context.Database.SqlQuery<string>($"SELECT {Secret} AS \"Value\" FROM pg_sleep({0.1})").ToListAsync(Ct);
        rows.ShouldBe([Secret]);
    }

    private ProbeHarness Harness(Action<DatabaseDiagnosticsOptions> configure) => new(
        database.ConnectionString,
        services: s =>
        {
            s.Configure(configure);
            s.AddLogging(logging => logging.AddFakeLogging());
        });
}

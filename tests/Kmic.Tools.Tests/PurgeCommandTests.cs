using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Tools.Tests;

/// <summary>
/// 安全事件表属于 Identity 模块，这里还没有，用框架测试库建一张同结构的
/// </summary>
public sealed class PurgeCommandTests(ToolsTemplate template) : IAsyncLifetime
{
    private readonly ClassDatabase<ToolsTemplate> _database = new(template);

    public ValueTask InitializeAsync() => _database.InitializeAsync();

    public ValueTask DisposeAsync() => _database.DisposeAsync();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task DeletesOnlyExpiredRows_AndReportsTheCount()
    {
        await FrameworkSchema.CreateAsync(_database.ConnectionString, Ct);
        var cutoff = ToolHarness.Now.AddDays(-90);
        foreach (var at in new[] { cutoff.AddDays(-1), cutoff.AddDays(-200), cutoff.AddDays(1), ToolHarness.Now })
        {
            await using var insert = _database.DataSource.CreateCommand(
                """INSERT INTO "Identity"."SecurityEvent" ("Id", "OccurredAt", "Type", "Severity", "TraceId") VALUES (gen_random_uuid(), @at, 'T', 0, 'x')""");
            insert.Parameters.AddWithValue("at", at);
            await insert.ExecuteNonQueryAsync(Ct);
        }

        var (exitCode, output) = await ToolHarness.RunnerAsync(
            _database.ConnectionString, false, "purge", "--table", "security-event", "--older-than-days", "90", "--batch-size", "1");

        exitCode.ShouldBe(0, output);
        output.ShouldContain("删除了 2 行");
        await using var left = _database.DataSource.CreateCommand("""SELECT count(*) FROM "Identity"."SecurityEvent" """);
        (await left.ExecuteScalarAsync(Ct)).ShouldBe(2L);
    }

    [Fact]
    public async Task TableOfAModuleNotBuiltYet_SaysSo()
    {
        var (exitCode, output) = await ToolHarness.RunnerAsync(_database.ConnectionString, false, "purge", "--table", "audit-log", "--older-than-days", "30");

        exitCode.ShouldBe(0, output);
        output.ShouldContain("还不存在");
    }
}

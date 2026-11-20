using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Tools.Tests;

/// <summary>
/// 写入数据库已分配容量：真实数据库，先用迁移程序建好结构，再跑运维命令
/// </summary>
public sealed class OperationsOperatorTests(ToolsTemplate template) : IAsyncLifetime
{
    private const string Key = "Operations.MessagingCapacity";
    private const long TwentyGiB = 21_474_836_480;

    private readonly ClassDatabase<ToolsTemplate> _database = new(template);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();
        (await ToolHarness.RunnerAsync(_database.ConnectionString, production: false, "migrate")).ExitCode.ShouldBe(0);
    }

    public ValueTask DisposeAsync() => _database.DisposeAsync();

    [Fact]
    public async Task SetMessagingCapacity_SavesTheSetting_AndRecordsTheChange()
    {
        var (exitCode, output) = await SetAsync(TwentyGiB);

        exitCode.ShouldBe(0, output);
        (await ScalarAsync($"""SELECT "Value" FROM "Configuration"."SystemSettingValue" WHERE "Key" = '{Key}'""")).ShouldBe("""{"capacityBytes": 21474836480}""");
        (await CountAsync($"""SELECT count(*) FROM "Configuration"."SystemSettingChange" WHERE "Key" = '{Key}' AND "OldValue" IS NULL""")).ShouldBe(1);
    }

    [Fact]
    public async Task SetMessagingCapacity_WithANewValue_UpdatesItAndKeepsTheOldValueInTheHistory()
    {
        (await SetAsync(TwentyGiB)).ExitCode.ShouldBe(0);

        (await SetAsync(TwentyGiB * 2)).ExitCode.ShouldBe(0);

        (await ScalarAsync($"""SELECT "Value" FROM "Configuration"."SystemSettingValue" WHERE "Key" = '{Key}'""")).ShouldBe("""{"capacityBytes": 42949672960}""");
        (await CountAsync($$"""SELECT count(*) FROM "Configuration"."SystemSettingChange" WHERE "Key" = '{{Key}}' AND "OldValue"::text = '{"capacityBytes": 21474836480}'""")).ShouldBe(1);
    }

    [Fact]
    public async Task SetMessagingCapacity_WithTheSameValue_MovesTheUpdateTimeForwardWithoutANewHistoryRow()
    {
        (await SetAsync(TwentyGiB)).ExitCode.ShouldBe(0);
        await ExecuteAsync("""UPDATE "Configuration"."SystemSettingValue" SET "UpdatedAt" = clock_timestamp() - interval '1 hour'""");
        var rewound = await UpdatedAtAsync();

        (await SetAsync(TwentyGiB)).ExitCode.ShouldBe(0);

        (await UpdatedAtAsync()).ShouldBeGreaterThan(rewound, "据此判断容量有没有过期，每小时写一次，值没变时也要往前走");
        (await CountAsync($"""SELECT count(*) FROM "Configuration"."SystemSettingChange" WHERE "Key" = '{Key}'""")).ShouldBe(1);
    }

    [Theory]
    [InlineData(1000L)]
    [InlineData(-5L)]
    public async Task SetMessagingCapacity_WithAnImplausibleValue_IsRefusedAndWritesNothing(long bytes)
    {
        var (exitCode, output) = await SetAsync(bytes);

        exitCode.ShouldBe(2);
        output.ShouldContain("1 GiB");
        (await CountAsync("""SELECT count(*) FROM "Configuration"."SystemSettingValue" """)).ShouldBe(0);
    }

    [Fact]
    public async Task SetMessagingCapacity_WithoutTheBytesOption_IsAUsageError()
    {
        var (exitCode, _) = await ToolHarness.OperatorAsync(_database.ConnectionString, string.Empty, "operations", "set-messaging-capacity");

        exitCode.ShouldBe(2);
    }

    private Task<(int ExitCode, string Output)> SetAsync(long bytes) =>
        ToolHarness.OperatorAsync(_database.ConnectionString, string.Empty, "operations", "set-messaging-capacity", "--bytes", bytes.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private async Task ExecuteAsync(string sql)
    {
        await using var command = _database.DataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<long> CountAsync(string sql)
    {
        await using var command = _database.DataSource.CreateCommand(sql);
        return (long)(await command.ExecuteScalarAsync(Ct) ?? 0L);
    }

    private async Task<string?> ScalarAsync(string sql)
    {
        await using var command = _database.DataSource.CreateCommand(sql);
        return (await command.ExecuteScalarAsync(Ct))?.ToString();
    }

    private async Task<DateTimeOffset> UpdatedAtAsync()
    {
        await using var command = _database.DataSource.CreateCommand($"""SELECT COALESCE("UpdatedAt", "CreatedAt") FROM "Configuration"."SystemSettingValue" WHERE "Key" = '{Key}'""");
        await using var reader = await command.ExecuteReaderAsync(Ct);
        (await reader.ReadAsync(Ct)).ShouldBeTrue();
        return await reader.GetFieldValueAsync<DateTimeOffset>(0, Ct);
    }
}

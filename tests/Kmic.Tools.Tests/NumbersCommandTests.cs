using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Tools.Tests;

/// <summary>
/// 结构用迁移程序自己建立，和部署顺序一致：先迁移，再导入数据、抬高计数
/// </summary>
public sealed class NumbersCommandTests(ToolsTemplate template) : IAsyncLifetime
{
    private const string Rule = "Sample.Specimen";

    private readonly ClassDatabase<ToolsTemplate> _database = new(template);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();
        (await ToolHarness.RunnerAsync(_database.ConnectionString, production: false, "migrate")).ExitCode.ShouldBe(0);
    }

    public ValueTask DisposeAsync() => _database.DisposeAsync();

    [Fact]
    public async Task Rules_ListsTheDeclaredRules()
    {
        var (exitCode, output) = await ToolHarness.RunnerAsync(_database.ConnectionString, production: false, "numbers", "rules");

        exitCode.ShouldBe(0, output);
        output.ShouldContain(Rule);
        output.ShouldContain("SP-{yyyy}{MM}-{seq:4}");
    }

    [Fact]
    public async Task EnsureAtLeast_RaisesTheCounter_NeverLowersIt()
    {
        var first = await ToolHarness.RunnerAsync(_database.ConnectionString, false, "numbers", "ensure-at-least", "--rule", Rule, "--value", "57", "--date", "2030-05-02");
        first.ExitCode.ShouldBe(0, first.Output);
        first.Output.ShouldContain("现在是 57");
        first.Output.ShouldContain("下一个序号是 58");
        (await CounterAsync("203005")).ShouldBe(57);

        var lower = await ToolHarness.RunnerAsync(_database.ConnectionString, false, "numbers", "ensure-at-least", "--rule", Rule, "--value", "10", "--date", "2030-05-20");
        lower.ExitCode.ShouldBe(0, lower.Output);
        lower.Output.ShouldContain("现在是 57");
        (await CounterAsync("203005")).ShouldBe(57);

        // 别的周期不受影响
        (await CounterAsync("203006")).ShouldBeNull();
    }

    [Theory]
    [InlineData(2, "numbers", "ensure-at-least", "--rule", "Nope.Rule", "--value", "5", "--date", "2030-05-02")]
    [InlineData(2, "numbers", "ensure-at-least", "--rule", Rule, "--value", "5")]
    [InlineData(2, "numbers", "ensure-at-least", "--rule", Rule, "--value", "5", "--date", "2030/05/02")]
    [InlineData(1, "numbers", "ensure-at-least", "--rule", Rule, "--value", "0", "--date", "2030-05-02")]
    [InlineData(1, "numbers", "ensure-at-least", "--rule", Rule, "--value", "5", "--date", "2030-05-02", "--partition", "A")]
    public async Task EnsureAtLeast_BadArguments_ExplainedAndNothingWritten(int expectedExitCode, params string[] args)
    {
        var (exitCode, _) = await ToolHarness.RunnerAsync(_database.ConnectionString, false, args);

        exitCode.ShouldBe(expectedExitCode);
        await using var count = _database.DataSource.CreateCommand("""SELECT count(*) FROM "Sample"."NumberCounter" """);
        (await count.ExecuteScalarAsync(Ct)).ShouldBe(0L);
    }

    [Fact]
    public async Task NoConnectionString_FailsWithTheKeyName()
    {
        using var output = new StringWriter();
        var exitCode = await MigrationRunner.RunnerApp.RunAsync(
            ["numbers", "ensure-at-least", "--rule", Rule, "--value", "5", "--date", "2030-05-02"],
            ToolHarness.Configuration(MigrationRunner.RunnerApp.ConnectionStringKey, null), ToolHarness.Modules, false, output, TimeProvider.System, Ct);

        exitCode.ShouldBe(1);
        output.ToString().ShouldContain(MigrationRunner.RunnerApp.ConnectionStringKey);
    }

    private async Task<long?> CounterAsync(string period)
    {
        await using var command = _database.DataSource.CreateCommand("""SELECT "LastValue" FROM "Sample"."NumberCounter" WHERE "Rule" = @rule AND "PeriodKey" = @period""");
        command.Parameters.AddWithValue("rule", Rule);
        command.Parameters.AddWithValue("period", period);
        return (long?)await command.ExecuteScalarAsync(Ct);
    }
}

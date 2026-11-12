using Kmic.BuildingBlocks.Testing.Database;
using Npgsql;
using Shouldly;

namespace Kmic.Tools.Tests;

public sealed class MigrationRunnerTests(ToolsTemplate template) : IAsyncLifetime
{
    private readonly ClassDatabase<ToolsTemplate> _database = new(template);

    public ValueTask InitializeAsync() => _database.InitializeAsync();

    public ValueTask DisposeAsync() => _database.DisposeAsync();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Status_FreshDatabase_ListsPending_ExitsWith3_ChangesNothing()
    {
        var (exitCode, output) = await ToolHarness.RunnerAsync(_database.ConnectionString, production: false, "status");

        exitCode.ShouldBe(3);
        output.ShouldContain("Sample/");
        await using var schemas = _database.DataSource.CreateCommand("SELECT count(*) FROM information_schema.schemata WHERE schema_name = 'Sample'");
        (await schemas.ExecuteScalarAsync(Ct)).ShouldBe(0L);
    }

    [Fact]
    public async Task Migrate_BuildsTheSchema_ThenStatusIsClean_AndRerunIsHarmless()
    {
        var (exitCode, output) = await ToolHarness.RunnerAsync(_database.ConnectionString, production: false, "migrate");

        exitCode.ShouldBe(0, output);
        output.ShouldContain("迁移完成");
        await using var table = _database.DataSource.CreateCommand("""SELECT count(*) FROM "Sample"."Specimen" """);
        (await table.ExecuteScalarAsync(Ct)).ShouldBe(0L);

        (await ToolHarness.RunnerAsync(_database.ConnectionString, production: false, "status")).ExitCode.ShouldBe(0);
        var again = await ToolHarness.RunnerAsync(_database.ConnectionString, production: false, "migrate");
        again.ExitCode.ShouldBe(0);
        again.Output.ShouldContain("没有未执行的迁移");
    }

    [Fact]
    public async Task Production_SkipsTheReferenceModule()
    {
        var (exitCode, output) = await ToolHarness.RunnerAsync(_database.ConnectionString, production: true, "status");

        exitCode.ShouldBe(0, output);
        output.ShouldContain("没有未执行的迁移");
    }

    [Fact]
    public async Task NoConnectionString_FailsWithTheKeyName()
    {
        using var output = new StringWriter();
        var exitCode = await MigrationRunner.RunnerApp.RunAsync(
            ["status"], ToolHarness.Configuration(MigrationRunner.RunnerApp.ConnectionStringKey, null), ToolHarness.Modules, false, output, TimeProvider.System, Ct);

        exitCode.ShouldBe(1);
        output.ToString().ShouldContain(MigrationRunner.RunnerApp.ConnectionStringKey);
    }

    [Fact]
    public async Task UnreachableDatabase_FailsWithMessage_NotAStackTrace()
    {
        var unreachable = new NpgsqlConnectionStringBuilder(_database.ConnectionString) { Port = 1, Timeout = 2 }.ConnectionString;

        var (exitCode, output) = await ToolHarness.RunnerAsync(unreachable, production: false, "status");

        exitCode.ShouldBe(1);
        output.ShouldStartWith("失败：");
    }

    [Theory]
    [InlineData("purge", "--table", "audit-log")]
    [InlineData("purge", "--table", "ticket", "--older-than-days", "30")]
    [InlineData("purge", "--table", "audit-log", "--older-than-days", "0")]
    public async Task Purge_BadArguments_ExitWith2(params string[] args)
    {
        var (exitCode, _) = await ToolHarness.RunnerAsync(_database.ConnectionString, false, args);

        exitCode.ShouldBe(2);
    }
}

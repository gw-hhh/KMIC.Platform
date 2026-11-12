using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.Identity.Infrastructure;
using Kmic.Sample.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Time.Testing;

namespace Kmic.Tools.Tests;

/// <summary>
/// 调用命令行程序的入口并收集输出
/// </summary>
internal static class ToolHarness
{
    public static readonly DateTimeOffset Now = new(2030, 6, 15, 8, 0, 0, TimeSpan.Zero);

    public static ToolModules Modules => new ToolModules().Add<IdentityModule>().Add<SampleModule>(nonProductionOnly: true);

    public static IConfiguration Configuration(string key, string? connectionString) =>
        new ConfigurationBuilder().AddInMemoryCollection(connectionString is null ? [] : [new(key, connectionString)]).Build();

    public static async Task<(int ExitCode, string Output)> RunnerAsync(string connectionString, bool production, params string[] args)
    {
        using var output = new StringWriter();
        var exitCode = await MigrationRunner.RunnerApp.RunAsync(
            args, Configuration(MigrationRunner.RunnerApp.ConnectionStringKey, connectionString), Modules, production, output, new FakeTimeProvider(Now), TestContext.Current.CancellationToken);
        return (exitCode, output.ToString());
    }

    public static async Task<(int ExitCode, string Output)> OperatorAsync(params string[] args) => await OperatorAsync(null, string.Empty, args);

    public static async Task<(int ExitCode, string Output)> OperatorAsync(string? connectionString, string standardInput, params string[] args)
    {
        using var output = new StringWriter();
        using var input = new StringReader(standardInput);
        var exitCode = await Operator.OperatorApp.RunAsync(
            args, Configuration(Operator.OperatorApp.ConnectionStringKey, connectionString), input, output, new FakeTimeProvider(Now), TestContext.Current.CancellationToken);
        return (exitCode, output.ToString());
    }
}

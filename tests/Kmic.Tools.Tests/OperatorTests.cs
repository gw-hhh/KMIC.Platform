using Shouldly;

namespace Kmic.Tools.Tests;

public sealed class OperatorTests
{
    [Fact]
    public async Task Help_ExitsWith0()
    {
        var (exitCode, output) = await ToolHarness.OperatorAsync("--help");

        exitCode.ShouldBe(0, output);
        output.ShouldContain("运维命令");
    }

    [Fact]
    public async Task UnknownCommand_ExitsWith2()
    {
        var (exitCode, _) = await ToolHarness.OperatorAsync("no-such-command");

        exitCode.ShouldBe(2);
    }
}

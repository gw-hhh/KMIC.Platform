using System.Runtime.CompilerServices;
using Kmic.BuildingBlocks.Testing.Contracts;
using Shouldly;

namespace Kmic.Api.Tests.Contracts;

public sealed class MessageContractTests
{
    [Fact]
    public void EveryMessageContract_MatchesItsSnapshot()
    {
        var contracts = MessageContractSnapshots.FindContractAssemblies(typeof(Program).Assembly);

        var problems = MessageContractSnapshots.Check(contracts, SnapshotDirectory());

        problems.ShouldBeEmpty(string.Join(Environment.NewLine, problems));
    }

    // 快照文件需保存至源码目录，因此使用源文件路径而非构建输出目录
    private static string SnapshotDirectory([CallerFilePath] string source = "") => Path.Combine(Path.GetDirectoryName(source)!, "Snapshots");
}

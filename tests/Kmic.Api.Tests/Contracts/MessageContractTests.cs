using System.Runtime.CompilerServices;
using Kmic.BuildingBlocks.Testing.Contracts;
using Shouldly;

namespace Kmic.Api.Tests.Contracts;

/// <summary>
/// 宿主引用的每个模块（和框架）对外的集成消息，契约都有快照，改动要经过确认
/// </summary>
public sealed class MessageContractTests
{
    [Fact]
    public void EveryMessageContract_MatchesItsSnapshot()
    {
        var contracts = MessageContractSnapshots.FindContractAssemblies(typeof(Program).Assembly);

        var problems = MessageContractSnapshots.Check(contracts, SnapshotDirectory());

        problems.ShouldBeEmpty(string.Join(Environment.NewLine, problems));
    }

    // 快照要写回源码目录，所以用源文件的位置，不用输出目录
    private static string SnapshotDirectory([CallerFilePath] string source = "") => Path.Combine(Path.GetDirectoryName(source)!, "Snapshots");
}

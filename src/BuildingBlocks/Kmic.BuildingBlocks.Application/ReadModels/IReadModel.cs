using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.BuildingBlocks.Application.ReadModels;

/// <summary>跨模块只读副本（读模型）；仅允许通过 IReadModelWriter 更新。</summary>
public interface IReadModel
{
    /// <summary>实体 ID，与源数据 ID 一致。</summary>
    Guid Id { get; }

    long SourceVersion { get; }

    ReferenceStatus Status { get; }

    bool IsTerminal { get; }

    ReadModelTerminalReason? TerminalReason { get; }

    DateTimeOffset? TerminalAt { get; }

    /// <summary>最近一次扫描到本行的重建轮次。</summary>
    Guid? RebuildRunId { get; }

    /// <summary>最近同步时间，仅用于界面展示与问题排查。</summary>
    DateTimeOffset SyncedAt { get; }
}

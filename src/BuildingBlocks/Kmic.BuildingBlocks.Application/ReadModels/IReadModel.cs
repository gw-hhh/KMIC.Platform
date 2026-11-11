using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.BuildingBlocks.Application.ReadModels;

/// <summary>
/// 读模型：别的模块数据在本模块的副本
/// </summary>
/// <remarks>
/// 只能通过 IReadModelWriter 写入。按来源版本取最新，物理删除后成为终态，此后任何同步都不再修改它
/// </remarks>
public interface IReadModel
{
    /// <summary>
    /// ID，等于源数据的 ID
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// 来源版本
    /// </summary>
    long SourceVersion { get; }

    /// <summary>
    /// 状态
    /// </summary>
    ReferenceStatus Status { get; }

    /// <summary>
    /// 是否终态
    /// </summary>
    bool IsTerminal { get; }

    /// <summary>
    /// 成为终态的原因
    /// </summary>
    ReadModelTerminalReason? TerminalReason { get; }

    /// <summary>
    /// 成为终态的时间
    /// </summary>
    DateTimeOffset? TerminalAt { get; }

    /// <summary>
    /// 最近一次扫描到本行的重建轮次
    /// </summary>
    Guid? RebuildRunId { get; }

    /// <summary>
    /// 同步时间
    /// </summary>
    /// <remarks>
    /// 只用于显示和排查，不参与任何判断
    /// </remarks>
    DateTimeOffset SyncedAt { get; }
}

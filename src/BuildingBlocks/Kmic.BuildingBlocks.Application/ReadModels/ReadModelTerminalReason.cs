namespace Kmic.BuildingBlocks.Application.ReadModels;

/// <summary>
/// 读模型成为终态的原因
/// </summary>
public enum ReadModelTerminalReason
{
    /// <summary>
    /// 收到物理删除的快照
    /// </summary>
    PurgeEvent,

    /// <summary>
    /// 重建收尾时源端确认已不存在
    /// </summary>
    ConfirmedMissing,
}

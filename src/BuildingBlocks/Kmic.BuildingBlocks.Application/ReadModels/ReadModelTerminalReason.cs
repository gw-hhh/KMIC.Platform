namespace Kmic.BuildingBlocks.Application.ReadModels;

public enum ReadModelTerminalReason
{
    /// <summary>收到物理删除快照。</summary>
    PurgeEvent,

    /// <summary>重建收尾时源端确认已不存在。</summary>
    ConfirmedMissing,
}

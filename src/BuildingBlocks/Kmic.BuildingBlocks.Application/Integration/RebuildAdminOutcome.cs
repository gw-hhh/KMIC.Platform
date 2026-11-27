namespace Kmic.BuildingBlocks.Application.Integration;

public enum RebuildAdminOutcome
{
    Done,

    /// <summary>订阅不存在，或未声明读模型重建方式。</summary>
    UnknownSubscription,

    /// <summary>该订阅已有尚未结束的重建轮次。</summary>
    AlreadyRunning,

    /// <summary>不存在等待确认的已暂停轮次。</summary>
    NotAwaitingConfirmation,
}

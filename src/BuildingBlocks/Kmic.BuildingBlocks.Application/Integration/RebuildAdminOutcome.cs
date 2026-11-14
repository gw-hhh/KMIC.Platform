namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 读模型重建运维操作的结果
/// </summary>
public enum RebuildAdminOutcome
{
    /// <summary>
    /// 做完了
    /// </summary>
    Done,

    /// <summary>
    /// 没有这个订阅，或者订阅没有声明重建方式
    /// </summary>
    UnknownSubscription,

    /// <summary>
    /// 订阅已经有没结束的一轮
    /// </summary>
    AlreadyRunning,

    /// <summary>
    /// 没有暂停等确认的一轮
    /// </summary>
    NotAwaitingConfirmation,
}

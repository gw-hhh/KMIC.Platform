namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 请求马上执行一个定时任务的结果
/// </summary>
public enum ScheduledJobTriggerOutcome
{
    /// <summary>
    /// 已交给调度器
    /// </summary>
    Triggered = 0,

    /// <summary>
    /// 调度库里没有这个任务
    /// </summary>
    NotFound = 1,

    /// <summary>
    /// 这个任务正在某个实例上执行
    /// </summary>
    AlreadyRunning = 2,
}

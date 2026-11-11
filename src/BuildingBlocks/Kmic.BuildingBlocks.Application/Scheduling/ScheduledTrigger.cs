namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 定时任务是怎么触发的
/// </summary>
public enum ScheduledTrigger
{
    /// <summary>
    /// 按计划时间
    /// </summary>
    Schedule = 0,

    /// <summary>
    /// 管理员手动执行
    /// </summary>
    Manual = 1,

    /// <summary>
    /// 错过之后补执行
    /// </summary>
    Misfire = 2,
}

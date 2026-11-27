namespace Kmic.BuildingBlocks.Application.Scheduling;

public enum ScheduledTrigger
{
    /// <summary>按调度计划触发。</summary>
    Schedule = 0,

    /// <summary>管理员手动执行。</summary>
    Manual = 1,

    /// <summary>错过触发后的补偿执行。</summary>
    Misfire = 2,
}

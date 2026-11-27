namespace Kmic.BuildingBlocks.Application.Scheduling;

public enum ScheduledJobTriggerOutcome
{
    /// <summary>已提交至调度器。</summary>
    Triggered = 0,

    /// <summary>调度器中不存在该任务。</summary>
    NotFound = 1,

    /// <summary>任务正在执行、即将按计划触发，或已有手动触发在排队。</summary>
    AlreadyRunning = 2,
}

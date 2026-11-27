namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>定时任务单次触发上下文。</summary>
/// <param name="ScheduledFireTime">计划触发时刻（UTC）；命令应以此而非当前时间确定处理窗口。</param>
public sealed record ScheduledRun(DateTimeOffset ScheduledFireTime, ScheduledTrigger Trigger);

namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 定时任务的一次触发
/// </summary>
/// <param name="ScheduledFireTime">计划触发时刻（UTC）。命令要按它而不是当前时间决定处理范围</param>
/// <param name="Trigger">触发方式</param>
public sealed record ScheduledRun(DateTimeOffset ScheduledFireTime, ScheduledTrigger Trigger);

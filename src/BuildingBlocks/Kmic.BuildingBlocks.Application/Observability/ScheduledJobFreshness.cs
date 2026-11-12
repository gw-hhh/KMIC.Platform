namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 一个定时任务最近一次成功距今多久
/// </summary>
/// <param name="Task">任务名</param>
/// <param name="LastSuccessAge">距上次成功的时间</param>
public sealed record ScheduledJobFreshness(string Task, TimeSpan LastSuccessAge);

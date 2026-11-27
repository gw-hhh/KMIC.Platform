namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>定时任务距上次成功执行的时长。</summary>
public sealed record ScheduledJobFreshness(string Task, TimeSpan LastSuccessAge);

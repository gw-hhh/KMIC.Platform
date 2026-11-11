namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 运维改过的开关和执行时间，没改的项是空
/// </summary>
internal sealed record ScheduledJobSetting(bool? Enabled, string? Cron);

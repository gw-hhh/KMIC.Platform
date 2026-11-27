namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>定时任务在调度器中的运行状态。</summary>
/// <param name="EffectiveCron">当前 Cron，停用或未同步时为 null。</param>
/// <param name="NextFireTime">下次触发时刻，无触发器时为 null。</param>
public sealed record ScheduledJobInfo(
    string Name,
    string Module,
    string DisplayName,
    string DefaultCron,
    Misfire Misfire,
    string? EffectiveCron,
    DateTimeOffset? NextFireTime)
{
    /// <summary>是否处于调度启用状态；为 false 时表示已停用，仅支持手动触发。</summary>
    public bool IsScheduled => EffectiveCron is not null;
}

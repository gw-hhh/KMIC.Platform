using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Operations.Domain.Jobs;

/// <summary>
/// 运维对一个定时任务改过的开关与执行时间：只存改过的项，没改的项是空，跟着代码里的默认值走
/// </summary>
/// <remarks>
/// 一个任务一行，行里两项都是空就是“已恢复默认”（行留着，便于追溯）。任务从代码里删除后行也保留，
/// 回滚到还有这个任务的版本时，运维改过的设置还在。框架在同步触发器时读这张表，不写它
/// </remarks>
public sealed class ScheduledJobSetting : AggregateRoot<ScheduledJobSettingId>
{
    private ScheduledJobSetting(ScheduledJobSettingId id, string jobName)
        : base(id) => JobName = jobName;

    /// <summary>
    /// 任务的稳定名称，如 Identity.PurgeSessions
    /// </summary>
    public string JobName { get; private set; }

    /// <summary>
    /// 是否启用；空表示跟着代码里的默认值（启用）
    /// </summary>
    public bool? Enabled { get; private set; }

    /// <summary>
    /// 执行时间的 Cron 表达式；空表示跟着代码里的默认值
    /// </summary>
    public string? Cron { get; private set; }

    /// <summary>
    /// 两项都跟着默认值，也就是等于没有覆盖
    /// </summary>
    public bool IsDefault => Enabled is null && Cron is null;

    /// <summary>
    /// 第一次改某个任务
    /// </summary>
    public static ScheduledJobSetting Create(string jobName, bool? enabled, string? cron)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobName);

        var setting = new ScheduledJobSetting(ScheduledJobSettingId.New(), jobName);
        setting.Apply(enabled, cron);
        return setting;
    }

    /// <summary>
    /// 整体换成新的覆盖：空的项恢复成跟着默认值
    /// </summary>
    public void Apply(bool? enabled, string? cron)
    {
        if (cron is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cron);
        }

        Enabled = enabled;
        Cron = cron?.Trim();
    }

    /// <summary>
    /// 恢复默认：两项都清空，行保留
    /// </summary>
    public void RestoreDefault()
    {
        Enabled = null;
        Cron = null;
    }
}

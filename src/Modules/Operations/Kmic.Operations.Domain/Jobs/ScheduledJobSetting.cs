using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Operations.Domain.Jobs;

/// <summary>
/// 定时任务的自定义启用状态和执行计划
/// </summary>
/// <remarks>
/// 值为空时用代码默认值
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
    /// 是否启用；为 null 时使用默认值（启用）
    /// </summary>
    public bool? Enabled { get; private set; }

    /// <summary>
    /// Cron 表达式；为 null 时使用代码中的默认表达式
    /// </summary>
    public string? Cron { get; private set; }

    /// <summary>
    /// 是否全部使用默认设置
    /// </summary>
    public bool IsDefault => Enabled is null && Cron is null;

    /// <summary>
    /// 创建定时任务自定义设置
    /// </summary>
    public static ScheduledJobSetting Create(string jobName, bool? enabled, string? cron)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobName);

        var setting = new ScheduledJobSetting(ScheduledJobSettingId.New(), jobName);
        setting.Apply(enabled, cron);
        return setting;
    }

    /// <summary>
    /// 替换自定义设置，null 表示使用代码默认值
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
    /// 恢复默认设置，保留记录
    /// </summary>
    public void RestoreDefault()
    {
        Enabled = null;
        Cron = null;
    }
}

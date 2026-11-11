namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 定时任务的可选设置
/// </summary>
public sealed class ScheduleOptions
{
    internal Misfire Misfire { get; private set; } = Misfire.FireOnceNow;

    internal string? Display { get; private set; }

    /// <summary>
    /// 所有实例都停着、错过了触发时怎么办，默认补执行一次
    /// </summary>
    public ScheduleOptions OnMisfire(Misfire misfire)
    {
        Misfire = misfire;
        return this;
    }

    /// <summary>
    /// 管理界面上显示的名称
    /// </summary>
    public ScheduleOptions DisplayName(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        Display = displayName;
        return this;
    }
}

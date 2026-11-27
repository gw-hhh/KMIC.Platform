namespace Kmic.BuildingBlocks.Application.Scheduling;

public sealed class ScheduleOptions
{
    internal Misfire Misfire { get; private set; } = Misfire.FireOnceNow;

    internal string? Display { get; private set; }

    /// <summary>设置错过触发时的处理策略，默认补偿执行一次。</summary>
    public ScheduleOptions OnMisfire(Misfire misfire)
    {
        Misfire = misfire;
        return this;
    }

    /// <summary>设置管理界面展示名称。</summary>
    public ScheduleOptions DisplayName(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        Display = displayName;
        return this;
    }
}

namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 一个定时任务在调度库里的状态
/// </summary>
/// <param name="Name">稳定名称</param>
/// <param name="Module">所属模块；框架自带的任务是 Framework</param>
/// <param name="DisplayName">显示名</param>
/// <param name="DefaultCron">代码里的默认执行时间</param>
/// <param name="Misfire">错过触发时怎么办</param>
/// <param name="EffectiveCron">触发器上现在用的执行时间；没有触发器（已停用，或还没同步）时为空</param>
/// <param name="NextFireTime">下一次触发的时刻；没有触发器时为空</param>
public sealed record ScheduledJobInfo(
    string Name,
    string Module,
    string DisplayName,
    string DefaultCron,
    Misfire Misfire,
    string? EffectiveCron,
    DateTimeOffset? NextFireTime)
{
    /// <summary>
    /// 有没有触发器：没有就是停用状态，只能手动执行
    /// </summary>
    public bool IsScheduled => EffectiveCron is not null;
}

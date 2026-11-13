namespace Kmic.Operations.Application.ScheduledJobs.List;

/// <summary>
/// 定时任务列表里的一项：代码里的默认值、运维改过的覆盖值、实际生效的状态和最近一次执行
/// </summary>
/// <param name="Name">稳定名称</param>
/// <param name="Module">所属模块；框架自带的任务是 Framework</param>
/// <param name="DisplayName">显示名</param>
/// <param name="IsDeclared">代码里还有没有这个任务；已经删除的只剩覆盖设置，回滚后配置仍在</param>
/// <param name="DefaultCron">代码里的默认执行时间；已经删除的任务没有</param>
/// <param name="OverrideEnabled">运维设的开关；空表示没改，跟着默认值（启用）</param>
/// <param name="OverrideCron">运维设的执行时间；空表示没改，跟着默认值</param>
/// <param name="OverrideVersion">覆盖设置的版本号，改它时带回来；从来没改过的任务没有</param>
/// <param name="Enabled">按默认值和覆盖值，应该是启用还是停用</param>
/// <param name="Cron">按默认值和覆盖值，应该用的执行时间</param>
/// <param name="ActiveCron">调度器里触发器现在用的执行时间；停用或还没同步时为空</param>
/// <param name="NextFireTime">下一次触发的时刻；停用时为空</param>
/// <param name="InSync">调度器里的状态和“应该”是否一致；改了设置却同步失败时为否</param>
/// <param name="LastRun">最近一次执行；从没执行过为空</param>
/// <param name="AllowedActions">当前用户能对它做的动作：Edit、RestoreDefault、RunNow</param>
public sealed record ScheduledJobItem(
    string Name,
    string Module,
    string DisplayName,
    bool IsDeclared,
    string? DefaultCron,
    bool? OverrideEnabled,
    string? OverrideCron,
    long? OverrideVersion,
    bool Enabled,
    string? Cron,
    string? ActiveCron,
    DateTimeOffset? NextFireTime,
    bool InSync,
    ScheduledJobRunItem? LastRun,
    IReadOnlyList<string> AllowedActions);

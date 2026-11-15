namespace Kmic.Operations.Application.ScheduledJobs.List;

/// <summary>
/// 定时任务及其默认计划、自定义设置、实际调度状态和最近执行记录
/// </summary>
/// <param name="Name">稳定名称</param>
/// <param name="Module">所属模块；框架自带的任务是 Framework</param>
/// <param name="DisplayName">显示名</param>
/// <param name="IsDeclared">任务是否仍在代码中声明；已移除任务保留自定义设置，便于回滚</param>
/// <param name="DefaultCron">代码默认 Cron 表达式；已移除声明的任务为空</param>
/// <param name="OverrideEnabled">自定义启用状态；为 null 时使用默认值（启用）</param>
/// <param name="OverrideCron">自定义 Cron 表达式；为 null 时使用代码默认表达式</param>
/// <param name="OverrideVersion">自定义设置的版本号，修改时须提交；尚无自定义设置时为空</param>
/// <param name="Enabled">当前配置的启用状态</param>
/// <param name="Cron">当前配置的 Cron 表达式</param>
/// <param name="ActiveCron">调度器当前的 Cron 表达式，无触发器时为空</param>
/// <param name="NextFireTime">下次计划触发时间；无后续触发或无触发器时为空</param>
/// <param name="InSync">实际调度状态是否与当前配置一致</param>
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

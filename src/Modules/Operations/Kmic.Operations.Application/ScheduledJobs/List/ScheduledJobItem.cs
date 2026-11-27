namespace Kmic.Operations.Application.ScheduledJobs.List;

/// <summary>定时任务及其默认计划、自定义设置、实际调度状态和最近执行记录</summary>
/// <param name="Name">稳定名称</param>
/// <param name="Module">所属模块</param>
/// <param name="DisplayName">显示名</param>
/// <param name="IsDeclared">任务是否仍在代码中声明</param>
/// <param name="DefaultCron">代码默认 Cron，已移除声明的任务为空</param>
/// <param name="OverrideEnabled">自定义启用状态</param>
/// <param name="OverrideCron">自定义 Cron 表达式</param>
/// <param name="OverrideVersion">自定义设置的版本号，尚无自定义设置时为空</param>
/// <param name="Enabled">当前配置的启用状态</param>
/// <param name="Cron">当前配置的 Cron</param>
/// <param name="ActiveCron">调度器当前 Cron，无触发器时为空</param>
/// <param name="NextFireTime">下次计划触发时间，无后续触发或无触发器时为空</param>
/// <param name="InSync">调度状态是否与配置一致</param>
/// <param name="LastRun">最近执行记录，未执行过时为空</param>
/// <param name="AllowedActions">允许的操作列表：Edit、RestoreDefault、RunNow</param>
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

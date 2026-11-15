namespace Kmic.Operations.Presentation.ScheduledJobs;

/// <summary>
/// 整体替换定时任务自定义设置，null 表示使用默认值
/// </summary>
/// <param name="Enabled">是否启用，null 表示使用默认值（启用）</param>
/// <param name="Cron">Cron 表达式，Quartz 写法（带秒），按业务时区解释，两次执行至少隔 1 分钟；为 null 时使用代码中的默认表达式</param>
/// <param name="ExpectedVersion">自定义设置的预期版本号（列表中的 overrideVersion），尚无设置时不传；不匹配时返回 409 Common.VersionConflict</param>
public sealed record SetJobOverrideRequest(bool? Enabled, string? Cron, long? ExpectedVersion);

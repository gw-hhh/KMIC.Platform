namespace Kmic.Operations.Presentation.ScheduledJobs;

/// <summary>
/// 改定时任务开关和执行时间的请求：整体替换，空的项恢复成跟着默认值
/// </summary>
/// <param name="Enabled">是否启用；空表示跟着默认值（启用）</param>
/// <param name="Cron">执行时间的 Cron 表达式，Quartz 写法（带秒），按业务时区解释，两次执行至少隔 1 分钟；空表示跟着代码里的默认值</param>
/// <param name="ExpectedVersion">调用方看到的版本号（列表里的 overrideVersion）；这个任务还没改过时不给。别人改过就返回 409 Common.VersionConflict</param>
public sealed record SetJobOverrideRequest(bool? Enabled, string? Cron, long? ExpectedVersion);

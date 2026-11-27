namespace Kmic.Operations.Presentation.ScheduledJobs;

/// <param name="Enabled">是否启用</param>
/// <param name="Cron">Cron 表达式</param>
/// <param name="ExpectedVersion">自定义设置的预期版本号（列表中的 overrideVersion）</param>
public sealed record SetJobOverrideRequest(bool? Enabled, string? Cron, long? ExpectedVersion);

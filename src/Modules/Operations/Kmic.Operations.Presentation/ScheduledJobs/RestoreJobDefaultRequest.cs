namespace Kmic.Operations.Presentation.ScheduledJobs;

/// <summary>
/// 恢复定时任务默认值的请求
/// </summary>
/// <param name="ExpectedVersion">自定义设置的预期版本号（列表中的 overrideVersion）</param>
public sealed record RestoreJobDefaultRequest(long ExpectedVersion);

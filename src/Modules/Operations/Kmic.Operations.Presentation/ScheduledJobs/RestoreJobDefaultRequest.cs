namespace Kmic.Operations.Presentation.ScheduledJobs;

/// <summary>
/// 恢复定时任务默认值的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号（列表里的 overrideVersion）。别人改过就返回 409 Common.VersionConflict</param>
public sealed record RestoreJobDefaultRequest(long ExpectedVersion);

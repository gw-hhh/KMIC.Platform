namespace Kmic.Operations.Presentation.ScheduledJobs;

/// <param name="ExpectedVersion">自定义设置的预期版本号（列表中的 overrideVersion）</param>
public sealed record RestoreJobDefaultRequest(long ExpectedVersion);

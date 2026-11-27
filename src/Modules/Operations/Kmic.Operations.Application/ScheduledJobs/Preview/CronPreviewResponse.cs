namespace Kmic.Operations.Application.ScheduledJobs.Preview;

/// <summary>Cron 触发时间预览</summary>
/// <param name="NextFireTimes">后续触发时间</param>
public sealed record CronPreviewResponse(IReadOnlyList<DateTimeOffset> NextFireTimes);

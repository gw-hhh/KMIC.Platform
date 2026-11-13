namespace Kmic.Operations.Application.ScheduledJobs.Preview;

/// <summary>
/// 一个 Cron 表达式接下来的触发时间
/// </summary>
/// <param name="NextFireTimes">接下来 5 次触发的时刻，按时间顺序</param>
public sealed record CronPreviewResponse(IReadOnlyList<DateTimeOffset> NextFireTimes);

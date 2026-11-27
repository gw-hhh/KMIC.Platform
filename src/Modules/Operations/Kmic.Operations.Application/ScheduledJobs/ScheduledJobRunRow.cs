namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>框架定时任务执行记录只读查询结果</summary>
public sealed record ScheduledJobRunRow
{
    public Guid Id { get; init; }

    public string JobName { get; init; } = string.Empty;

    public DateTimeOffset ScheduledFireTime { get; init; }

    public DateTimeOffset StartedAt { get; init; }

    /// <summary>结束时间，执行中为空</summary>
    public DateTimeOffset? FinishedAt { get; init; }

    public long? DurationMs { get; init; }

    public string Trigger { get; init; } = string.Empty;

    public Guid? TriggeredBy { get; init; }

    public string Status { get; init; } = string.Empty;

    public int? ItemsProcessed { get; init; }

    public string? Note { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public string InstanceId { get; init; } = string.Empty;

    public string? TraceId { get; init; }
}

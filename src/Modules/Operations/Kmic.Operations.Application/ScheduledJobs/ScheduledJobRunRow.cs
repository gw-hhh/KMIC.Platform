namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 定时任务执行记录表里的一行，只用来读：记录由框架的调度器写，字段与表一一对应
/// </summary>
/// <remarks>
/// 状态与触发方式在查询里已经换成编码文字，见 <c>JobRunStatus</c>、<c>JobRunTrigger</c>
/// </remarks>
public sealed record ScheduledJobRunRow
{
    /// <summary>
    /// 记录 ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 任务名
    /// </summary>
    public string JobName { get; init; } = string.Empty;

    /// <summary>
    /// 计划触发的时刻
    /// </summary>
    public DateTimeOffset ScheduledFireTime { get; init; }

    /// <summary>
    /// 开始执行的时刻
    /// </summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// 结束的时刻；还在执行时为空
    /// </summary>
    public DateTimeOffset? FinishedAt { get; init; }

    /// <summary>
    /// 耗时，毫秒
    /// </summary>
    public long? DurationMs { get; init; }

    /// <summary>
    /// 触发方式的编码
    /// </summary>
    public string Trigger { get; init; } = string.Empty;

    /// <summary>
    /// 手动执行的人
    /// </summary>
    public Guid? TriggeredBy { get; init; }

    /// <summary>
    /// 状态的编码
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// 处理了多少条
    /// </summary>
    public int? ItemsProcessed { get; init; }

    /// <summary>
    /// 命令给的简短说明
    /// </summary>
    public string? Note { get; init; }

    /// <summary>
    /// 失败时的错误码
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// 失败时的错误说明
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 执行它的调度器实例
    /// </summary>
    public string InstanceId { get; init; } = string.Empty;

    /// <summary>
    /// 链路追踪 ID
    /// </summary>
    public string? TraceId { get; init; }
}

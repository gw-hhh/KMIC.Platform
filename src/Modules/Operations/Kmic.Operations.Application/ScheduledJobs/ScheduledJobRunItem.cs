using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 定时任务的一次执行
/// </summary>
/// <param name="Id">记录 ID</param>
/// <param name="JobName">任务名</param>
/// <param name="ScheduledFireTime">计划触发的时刻</param>
/// <param name="StartedAt">开始执行的时刻</param>
/// <param name="FinishedAt">结束的时刻；还在执行时为空</param>
/// <param name="DurationMs">耗时，毫秒</param>
/// <param name="Trigger">怎么触发的：按计划、手动、补执行</param>
/// <param name="TriggeredBy">手动执行的人</param>
/// <param name="TriggeredByName">手动执行的人的名称；用户已经不存在时为空</param>
/// <param name="Status">状态</param>
/// <param name="ItemsProcessed">处理了多少条</param>
/// <param name="Note">命令给的简短说明</param>
/// <param name="ErrorCode">失败时的错误码</param>
/// <param name="ErrorMessage">失败时的错误说明</param>
/// <param name="InstanceId">执行它的调度器实例</param>
/// <param name="TraceId">链路追踪 ID，可以到日志与链路里查这次执行</param>
public sealed record ScheduledJobRunItem(
    Guid Id,
    string JobName,
    DateTimeOffset ScheduledFireTime,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    long? DurationMs,
    JobRunTrigger Trigger,
    Guid? TriggeredBy,
    string? TriggeredByName,
    JobRunStatus Status,
    int? ItemsProcessed,
    string? Note,
    string? ErrorCode,
    string? ErrorMessage,
    string InstanceId,
    string? TraceId);

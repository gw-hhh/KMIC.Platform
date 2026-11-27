using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>定时任务执行记录项。</summary>
/// <param name="Id">记录 ID</param>
/// <param name="JobName">任务名</param>
/// <param name="ScheduledFireTime">计划触发时间</param>
/// <param name="StartedAt">开始时间</param>
/// <param name="FinishedAt">结束时间，执行中为空</param>
/// <param name="DurationMs">耗时</param>
/// <param name="Trigger">触发方式</param>
/// <param name="TriggeredBy">手动触发用户 ID</param>
/// <param name="TriggeredByName">手动触发用户名称，用户不存在时为空</param>
/// <param name="Status">状态</param>
/// <param name="ItemsProcessed">已处理条数</param>
/// <param name="Note">执行结果说明</param>
/// <param name="ErrorCode">失败时的错误码</param>
/// <param name="ErrorMessage">失败时的错误说明</param>
/// <param name="InstanceId">执行任务的调度器实例</param>
/// <param name="TraceId">链路追踪 ID</param>
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

using Kmic.BuildingBlocks.Contracts.References;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 将定时任务执行记录转换为响应，并补充手动触发用户名称
/// </summary>
internal static class ScheduledJobRunItems
{
    /// <summary>
    /// 收集手动触发用户 ID，供批量解析名称
    /// </summary>
    public static IReadOnlyCollection<Guid> UsersOf(IEnumerable<ScheduledJobRunRow> rows) =>
        [.. rows.Select(r => r.TriggeredBy).OfType<Guid>().Distinct()];

    public static ScheduledJobRunItem From(ScheduledJobRunRow row, IReadOnlyDictionary<Guid, ReferenceInfo> names) =>
        new(
            row.Id,
            row.JobName,
            row.ScheduledFireTime,
            row.StartedAt,
            row.FinishedAt,
            row.DurationMs,
            JobRunTrigger.FromCode(row.Trigger),
            row.TriggeredBy,
            row.TriggeredBy is { } by ? names.GetValueOrDefault(by)?.DisplayName : null,
            JobRunStatus.FromCode(row.Status),
            row.ItemsProcessed,
            row.Note,
            row.ErrorCode,
            row.ErrorMessage,
            row.InstanceId,
            row.TraceId);
}

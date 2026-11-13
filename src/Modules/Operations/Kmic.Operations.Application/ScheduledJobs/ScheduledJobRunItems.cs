using Kmic.BuildingBlocks.Contracts.References;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 把执行记录行换成响应里的一项，并补上手动执行的人的名称
/// </summary>
internal static class ScheduledJobRunItems
{
    /// <summary>
    /// 手动执行的人要解析名称，先把需要解析的用户收集起来
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

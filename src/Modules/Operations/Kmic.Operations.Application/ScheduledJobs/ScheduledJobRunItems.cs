using Kmic.BuildingBlocks.Contracts.References;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs;

internal static class ScheduledJobRunItems
{
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

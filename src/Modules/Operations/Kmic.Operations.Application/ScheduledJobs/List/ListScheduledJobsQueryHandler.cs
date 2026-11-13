using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Operations.Application.ScheduledJobs.List;

/// <summary>
/// 处理 <see cref="ListScheduledJobsQuery"/>
/// </summary>
public sealed class ListScheduledJobsQueryHandler(
    IOperationsQueryDb db,
    IScheduledJobControl control,
    IUserReferences users,
    IAllowedActions<ScheduledJobTarget> allowedActions) : IQueryHandler<ListScheduledJobsQuery, IReadOnlyList<ScheduledJobItem>>
{
    // 代码里的默认值总是启用
    private const bool DefaultEnabled = true;

    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyList<ScheduledJobItem>>> Handle(ListScheduledJobsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var declared = await control.ListAsync(cancellationToken);
        var overrides = await db.ScheduledJobSettings
            .Select(s => new Override(s.JobName, s.Enabled, s.Cron, s.ConcurrencyVersion))
            .ToDictionaryAsync(s => s.JobName, StringComparer.Ordinal, cancellationToken);
        var latest = await db.LatestScheduledJobRuns.ToDictionaryAsync(r => r.JobName, StringComparer.Ordinal, cancellationToken);
        var names = await users.ResolveAsync(ScheduledJobRunItems.UsersOf(latest.Values), cancellationToken);

        var items = declared
            .Select(job => Declared(job, overrides.GetValueOrDefault(job.Name), latest.GetValueOrDefault(job.Name), names))
            .Concat(overrides.Values.Where(o => declared.All(d => !string.Equals(d.Name, o.JobName, StringComparison.Ordinal))).Select(o => Retired(o, latest.GetValueOrDefault(o.JobName), names)))
            .OrderBy(i => i.Module, StringComparer.Ordinal)
            .ThenBy(i => i.Name, StringComparer.Ordinal)
            .ToList();
        return items;
    }

    private ScheduledJobItem Declared(ScheduledJobInfo job, Override? setting, ScheduledJobRunRow? run, IReadOnlyDictionary<Guid, ReferenceInfo> names)
    {
        var enabled = setting?.Enabled ?? DefaultEnabled;
        var cron = setting?.Cron ?? job.DefaultCron;
        var inSync = enabled == job.IsScheduled && (!enabled || string.Equals(cron, job.EffectiveCron, StringComparison.Ordinal));
        var target = new ScheduledJobTarget(job.Name, IsDeclared: true, HasOverride: setting is { Enabled: not null } or { Cron: not null });
        return new ScheduledJobItem(
            job.Name,
            job.Module,
            job.DisplayName,
            IsDeclared: true,
            job.DefaultCron,
            setting?.Enabled,
            setting?.Cron,
            setting?.Version,
            enabled,
            cron,
            job.EffectiveCron,
            job.NextFireTime,
            inSync,
            run is null ? null : ScheduledJobRunItems.From(run, names),
            allowedActions.For(target));
    }

    private static ScheduledJobItem Retired(Override setting, ScheduledJobRunRow? run, IReadOnlyDictionary<Guid, ReferenceInfo> names) =>
        new(
            setting.JobName,
            setting.JobName.Split('.')[0],
            setting.JobName,
            IsDeclared: false,
            DefaultCron: null,
            setting.Enabled,
            setting.Cron,
            setting.Version,
            Enabled: false,
            Cron: null,
            ActiveCron: null,
            NextFireTime: null,
            InSync: true,
            run is null ? null : ScheduledJobRunItems.From(run, names),
            []);

    private sealed record Override(string JobName, bool? Enabled, string? Cron, long Version);
}

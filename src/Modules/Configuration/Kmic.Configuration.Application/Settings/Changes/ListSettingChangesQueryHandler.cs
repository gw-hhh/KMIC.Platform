using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Application.Settings.Changes;

/// <summary>
/// 处理 <see cref="ListSettingChangesQuery"/>
/// </summary>
public sealed class ListSettingChangesQueryHandler(IConfigurationQueryDb db, IUserReferences users) : IQueryHandler<ListSettingChangesQuery, PagedResult<SettingChangeItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<PagedResult<SettingChangeItem>>> Handle(ListSettingChangesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var changes = db.SettingChanges.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Key))
        {
            changes = changes.Where(c => c.Key == query.Key);
        }

        if (query.From is { } from)
        {
            changes = changes.Where(c => EF.Property<DateTimeOffset>(c, AuditColumns.CreatedAt) >= from);
        }

        if (query.To is { } to)
        {
            changes = changes.Where(c => EF.Property<DateTimeOffset>(c, AuditColumns.CreatedAt) < to);
        }

        var page = await changes
            .Select(c => new SettingChangeRow(
                c.Id, c.Key, c.OldValue, c.NewValue, c.Reason, EF.Property<UserId>(c, AuditColumns.CreatedBy), EF.Property<DateTimeOffset>(c, AuditColumns.CreatedAt), c.TraceId))
            .ToPagedAsync(query.Page, query.Sort, SettingChangeSort.Map, cancellationToken);
        if (page.IsFailure)
        {
            return page.Error;
        }

        var paged = page.Value;
        var names = await users.ResolveAsync([.. paged.Items.Select(i => i.ChangedBy.Value).Distinct()], cancellationToken);
        return new PagedResult<SettingChangeItem>(
            [
                .. paged.Items.Select(row => new SettingChangeItem(
                    row.Id.Value,
                    row.Key,
                    row.OldValue is null ? null : DeclaredValues.ElementOf(row.OldValue),
                    row.NewValue is null ? null : DeclaredValues.ElementOf(row.NewValue),
                    row.Reason,
                    row.ChangedBy,
                    names.GetValueOrDefault(row.ChangedBy.Value)?.DisplayName,
                    row.ChangedAt,
                    row.TraceId)),
            ],
            paged.Page,
            paged.PageSize,
            paged.TotalCount,
            paged.TotalCountExact);
    }
}

using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;
using Kmic.Operations.Domain.Audit;

namespace Kmic.Operations.Application.Audit.List;

/// <summary>
/// 处理 <see cref="ListAuditQuery"/>
/// </summary>
public sealed class ListAuditQueryHandler(IOperationsQueryDb db, ICursorProtector protector, IUserReferences users)
    : IQueryHandler<ListAuditQuery, CursorPage<AuditListItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<CursorPage<AuditListItem>>> Handle(ListAuditQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var audits = AuditFilters.Apply(db, query.From, query.To, query.ActorUserId, query.ActualUserId, query.Module, query.Action, query.ResourceType, query.ResourceId);

        var page = await audits
            .Select(a => new AuditRow(a.Id, a.OccurredAt, a.Module, a.Action, a.Origin, a.ActorUserId, a.ActualUserId, a.TraceId, a.Targets))
            .ToCursorPageAsync(query.Cursor, protector, r => r.OccurredAt, r => r.Id, descending: true, cancellationToken);
        if (page.IsFailure)
        {
            return page.Error;
        }

        var rows = page.Value.Items;
        var names = await users.ResolveAsync(
            [.. rows.SelectMany(r => new[] { r.ActorUserId?.Value, r.ActualUserId?.Value }).OfType<Guid>().Distinct()],
            cancellationToken);
        var items = rows.Select(r => new AuditListItem(
            r.Id.Value,
            r.OccurredAt,
            r.Module,
            r.Action,
            r.Origin,
            r.ActorUserId?.Value,
            r.ActorUserId is { } a ? names.GetValueOrDefault(a.Value)?.DisplayName : null,
            r.ActualUserId?.Value,
            r.ActualUserId is { } b ? names.GetValueOrDefault(b.Value)?.DisplayName : null,
            r.TraceId,
            JsonSerializer.Deserialize<List<AuditTargetItem>>(r.Targets, KmicJsonOptions.Stored) ?? []));
        return new CursorPage<AuditListItem>([.. items], page.Value.NextCursor);
    }

    private sealed record AuditRow(
        AuditLogId Id,
        DateTimeOffset OccurredAt,
        string Module,
        string Action,
        string Origin,
        UserId? ActorUserId,
        UserId? ActualUserId,
        string TraceId,
        string Targets);
}

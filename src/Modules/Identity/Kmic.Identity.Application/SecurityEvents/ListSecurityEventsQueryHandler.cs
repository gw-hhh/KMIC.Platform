using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.SecurityEvents;

/// <summary>
/// 处理 <see cref="ListSecurityEventsQuery"/>
/// </summary>
public sealed class ListSecurityEventsQueryHandler(IIdentityQueryDb db) : IQueryHandler<ListSecurityEventsQuery, PagedResult<SecurityEventItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<PagedResult<SecurityEventItem>>> Handle(ListSecurityEventsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var events = db.SecurityEvents.Where(e => e.OccurredAt >= query.From && e.OccurredAt < query.To);
        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            events = events.Where(e => e.Type == query.Type);
        }

        if (query.UserId is { } userId)
        {
            events = events.Where(e => e.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(query.ClientIp))
        {
            events = events.Where(e => e.ClientIp == query.ClientIp);
        }

        if (!string.IsNullOrWhiteSpace(query.Account))
        {
            events = events.Where(e => e.AttemptedAccount == query.Account);
        }

        var page = await events.ToPagedAsync(query.Page, query.Sort, SecurityEventSort.Map, cancellationToken);
        if (page.IsFailure)
        {
            return page.Error;
        }

        var paged = page.Value;
        return new PagedResult<SecurityEventItem>([.. paged.Items.Select(ToItem)], paged.Page, paged.PageSize, paged.TotalCount, paged.TotalCountExact);
    }

    private static SecurityEventItem ToItem(SecurityEventRow row) =>
        new(row.Id, row.OccurredAt, row.Type, (SecuritySeverity)row.Severity, row.Reason, row.UserId, row.ActualUserId, row.RoleId, row.SessionId, row.AttemptedAccount, row.ClientIp, row.ResourceType, row.ResourceId, ParseDetails(row.Details), row.TraceId);

    private static Dictionary<string, string>? ParseDetails(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(json);
}

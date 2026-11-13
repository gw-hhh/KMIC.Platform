using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Deliveries;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Application.Deliveries.List;

/// <summary>
/// 处理 <see cref="ListDeliveriesQuery"/>
/// </summary>
public sealed class ListDeliveriesQueryHandler(INotificationsQueryDb db) : IQueryHandler<ListDeliveriesQuery, PagedResult<DeliveryListItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<PagedResult<DeliveryListItem>>> Handle(ListDeliveriesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var deliveries = db.Deliveries;
        if (query.RecipientUserId is { } user)
        {
            var recipient = UserId.From(user);
            deliveries = deliveries.Where(d => d.RecipientUserId == recipient);
        }

        if (query.Channel is { } channelCode)
        {
            var channel = DeliveryChannel.FromCode(channelCode);
            deliveries = deliveries.Where(d => d.Channel == channel);
        }

        if (query.Status is { } statusCode)
        {
            var status = DeliveryStatus.FromCode(statusCode);
            deliveries = deliveries.Where(d => d.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.TemplateKey))
        {
            deliveries = deliveries.Where(d => d.TemplateKey == query.TemplateKey);
        }

        if (query.From is { } from)
        {
            deliveries = deliveries.Where(d => EF.Property<DateTimeOffset>(d, AuditColumns.CreatedAt) >= from);
        }

        if (query.To is { } to)
        {
            deliveries = deliveries.Where(d => EF.Property<DateTimeOffset>(d, AuditColumns.CreatedAt) < to);
        }

        return await deliveries
            .Select(d => new DeliveryListItem(
                d.Id,
                d.Channel,
                d.Status,
                d.SkipReason,
                d.RecipientUserId,
                d.AddressMasked,
                d.TemplateKey,
                d.TemplateVersion,
                EF.Property<int>(d, DeliveryColumns.Attempts),
                EF.Property<string?>(d, DeliveryColumns.LastError),
                EF.Property<DateTimeOffset?>(d, DeliveryColumns.SentAt),
                EF.Property<DateTimeOffset>(d, AuditColumns.CreatedAt),
                EF.Property<DateTimeOffset?>(d, DeliveryColumns.AbandonedAt) != null))
            .ToPagedAsync(query.Page, query.Sort, DeliverySort.Map, cancellationToken);
    }
}

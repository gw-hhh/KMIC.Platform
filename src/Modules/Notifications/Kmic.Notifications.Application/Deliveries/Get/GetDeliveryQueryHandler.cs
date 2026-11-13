using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Application.Deliveries.Get;

/// <summary>
/// 处理 <see cref="GetDeliveryQuery"/>
/// </summary>
public sealed class GetDeliveryQueryHandler(INotificationsQueryDb db) : IQueryHandler<GetDeliveryQuery, DeliveryDetailResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<DeliveryDetailResponse>> Handle(GetDeliveryQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var delivery = await db.Deliveries
            .Where(d => d.Id == query.Id)
            .Select(d => new DeliveryDetailResponse(
                d.Id,
                d.NotificationId,
                d.Channel,
                d.Role,
                d.Status,
                d.SkipReason,
                d.RecipientUserId,
                d.AddressMasked,
                d.Subject,
                d.Body,
                d.TemplateKey,
                d.TemplateVersion,
                d.Priority,
                EF.Property<int>(d, DeliveryColumns.Attempts),
                EF.Property<DateTimeOffset?>(d, DeliveryColumns.NextAttemptAt),
                EF.Property<string?>(d, DeliveryColumns.LastError),
                EF.Property<string?>(d, DeliveryColumns.ProviderMessageId),
                EF.Property<DateTimeOffset?>(d, DeliveryColumns.SentAt),
                EF.Property<DateTimeOffset>(d, AuditColumns.CreatedAt),
                EF.Property<DateTimeOffset?>(d, DeliveryColumns.AbandonedAt),
                EF.Property<string?>(d, DeliveryColumns.AbandonReason)))
            .FirstOrDefaultAsync(cancellationToken);
        return delivery is null ? NotificationsErrors.DeliveryNotFound : delivery;
    }
}

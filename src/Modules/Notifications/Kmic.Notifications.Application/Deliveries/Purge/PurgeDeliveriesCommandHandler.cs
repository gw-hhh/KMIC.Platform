using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Notifications;
using Microsoft.Extensions.Options;

namespace Kmic.Notifications.Application.Deliveries.Purge;

/// <summary>
/// 处理 <see cref="PurgeDeliveriesCommand"/>
/// </summary>
public sealed class PurgeDeliveriesCommandHandler(IDeliveryRepository deliveries, INotificationRepository notifications, IOptions<NotificationsRetentionOptions> options, TimeProvider time)
    : ICommandHandler<PurgeDeliveriesCommand, ScheduledJobOutcome>
{
    /// <inheritdoc />
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(PurgeDeliveriesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var before = time.GetUtcNow().AddDays(-options.Value.DeliveryDays);
        var purged = await deliveries.PurgeAsync(before, cancellationToken);
        purged += await notifications.PurgeAsync(before, cancellationToken);
        return new ScheduledJobOutcome(purged);
    }
}

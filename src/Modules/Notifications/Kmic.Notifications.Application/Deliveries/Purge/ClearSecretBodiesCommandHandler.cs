using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Deliveries.Purge;

/// <summary>
/// 处理 <see cref="ClearSecretBodiesCommand"/>
/// </summary>
public sealed class ClearSecretBodiesCommandHandler(IDeliveryRepository deliveries, NotificationTemplateCatalog catalog)
    : ICommandHandler<ClearSecretBodiesCommand, ScheduledJobOutcome>
{
    /// <inheritdoc />
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(ClearSecretBodiesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var secretKeys = catalog.All.Where(t => t.CarriesSecret).Select(t => t.Key).ToList();
        if (secretKeys.Count == 0)
        {
            return new ScheduledJobOutcome(0);
        }

        return new ScheduledJobOutcome(await deliveries.ClearSecretBodiesAsync(secretKeys, cancellationToken));
    }
}

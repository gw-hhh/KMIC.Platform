using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Errors;

namespace Kmic.Notifications.Application.Deliveries.Abandon;

/// <summary>
/// 放弃失败的投递
/// </summary>
public sealed class AbandonDeliveryCommandHandler(IDeliveryRepository deliveries, TimeProvider time) : ICommandHandler<AbandonDeliveryCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(AbandonDeliveryCommand command, CancellationToken cancellationToken)
    {
        var loaded = await deliveries.GetForUpdateAsync(command.Id, null, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        return await deliveries.AbandonAsync(command.Id, command.Reason.Trim(), time.GetUtcNow(), cancellationToken)
            ? Result.Success()
            : NotificationsErrors.DeliveryNotAbandonable;
    }
}

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Contracts;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Errors;

namespace Kmic.Notifications.Application.Deliveries.Retry;

/// <summary>
/// 处理 <see cref="RetryDeliveryCommand"/>
/// </summary>
public sealed class RetryDeliveryCommandHandler(IDeliveryRepository deliveries, NotificationTemplateCatalog catalog, TimeProvider time) : ICommandHandler<RetryDeliveryCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(RetryDeliveryCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await deliveries.GetForUpdateAsync(command.Id, null, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        if (loaded.Value.Status != DeliveryStatus.Failed)
        {
            return NotificationsErrors.DeliveryNotRetryable;
        }

        // 含密通知只在有效期内有意义，过了就不重发了
        DateTimeOffset? createdNotBefore = catalog.Find(loaded.Value.TemplateKey)?.CarriesSecret == true
            ? time.GetUtcNow() - TimeSpan.FromSeconds(SendSecretNotification.LifetimeSeconds)
            : null;
        if (await deliveries.RetryAsync(command.Id, createdNotBefore, cancellationToken))
        {
            return Result.Success();
        }

        return createdNotBefore is null ? NotificationsErrors.DeliveryNotRetryable : NotificationsErrors.DeliverySecretExpired;
    }
}

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Errors;

namespace Kmic.Notifications.Application.Deliveries.Retry;

public sealed class RetryDeliveryCommandHandler(IDeliveryRepository deliveries, NotificationTemplateCatalog catalog) : ICommandHandler<RetryDeliveryCommand>
{
    public async ValueTask<Result> Handle(RetryDeliveryCommand command, CancellationToken cancellationToken)
    {
        var loaded = await deliveries.GetForUpdateAsync(command.Id, null, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        if (loaded.Value.Status != DeliveryStatus.Failed)
        {
            return NotificationsErrors.DeliveryNotRetryable;
        }

        // 敏感通知最终失败时已清空正文与参数，无法重发，须由业务重新生成凭据发送
        if (catalog.Find(loaded.Value.TemplateKey)?.CarriesSecret == true)
        {
            return NotificationsErrors.DeliverySecretNotRetryable;
        }

        return await deliveries.RetryAsync(command.Id, cancellationToken) ? Result.Success() : NotificationsErrors.DeliveryNotRetryable;
    }
}

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Errors;

namespace Kmic.Notifications.Application.Deliveries.Retry;

/// <summary>
/// 将失败的投递重新加入发送队列
/// </summary>
public sealed class RetryDeliveryCommandHandler(IDeliveryRepository deliveries, NotificationTemplateCatalog catalog) : ICommandHandler<RetryDeliveryCommand>
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

        // 含密通知最终失败时正文和短信参数已经清空，重新排队也发不出正确内容；需要重发时由业务生成新凭据另发
        if (catalog.Find(loaded.Value.TemplateKey)?.CarriesSecret == true)
        {
            return NotificationsErrors.DeliverySecretNotRetryable;
        }

        return await deliveries.RetryAsync(command.Id, cancellationToken) ? Result.Success() : NotificationsErrors.DeliveryNotRetryable;
    }
}

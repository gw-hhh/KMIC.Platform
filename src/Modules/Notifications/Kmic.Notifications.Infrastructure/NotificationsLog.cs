using Microsoft.Extensions.Logging;

namespace Kmic.Notifications.Infrastructure;

internal static partial class NotificationsLog
{
    [LoggerMessage(EventId = 14001, Level = LogLevel.Warning, Message = "Calling the {Channel} channel for delivery {DeliveryId} failed, it is retried later")]
    public static partial void ChannelCallFailed(ILogger logger, Exception exception, Guid deliveryId, string channel);
}

using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 集成消息的日志
/// </summary>
internal static partial class MessagingLog
{
    [LoggerMessage(EventId = 2101, Level = LogLevel.Warning, Message = "Delivery of message {MessageId} ({MessageType}) from {Module} failed, attempt {Attempts}")]
    public static partial void DeliveryFailed(ILogger logger, Exception? exception, Guid messageId, string messageType, string module, int attempts);

    [LoggerMessage(EventId = 2102, Level = LogLevel.Error, Message = "Message {MessageId} ({MessageType}) from {Module} moved to dead letters after {Attempts} attempts")]
    public static partial void DeliveryDead(ILogger logger, Guid messageId, string messageType, string module, int attempts);

    [LoggerMessage(EventId = 2103, Level = LogLevel.Information, Message = "Message {MessageId} from {Module} was taken over by another deliverer, skipping")]
    public static partial void DeliveryTakenOver(ILogger logger, Guid messageId, string module);
}

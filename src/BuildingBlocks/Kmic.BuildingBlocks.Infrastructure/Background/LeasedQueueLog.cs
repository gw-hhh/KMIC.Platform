using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Background;

/// <summary>
/// 领取队列循环的日志
/// </summary>
internal static partial class LeasedQueueLog
{
    [LoggerMessage(EventId = 2901, Level = LogLevel.Warning, Message = "Database unavailable while claiming from queue {Queue}, retrying in {DelaySeconds} s")]
    public static partial void DatabaseUnavailable(ILogger logger, Exception exception, string queue, double delaySeconds);

    [LoggerMessage(EventId = 2902, Level = LogLevel.Error, Message = "Processing an item of queue {Queue} failed")]
    public static partial void ProcessingFailed(ILogger logger, Exception exception, string queue);
}

using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// 缓存的日志
/// </summary>
internal static partial class CacheLog
{
    [LoggerMessage(EventId = 2701, Level = LogLevel.Warning, Message = "Handling a cache invalidation from another instance failed, relying on expiry")]
    public static partial void RemoteInvalidationFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2702, Level = LogLevel.Warning, Message = "Invalidating cache tags after commit failed, relying on expiry")]
    public static partial void InvalidationFailed(ILogger logger, Exception exception);
}

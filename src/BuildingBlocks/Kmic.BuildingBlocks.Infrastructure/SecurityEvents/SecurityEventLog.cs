using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

/// <summary>
/// 安全事件的日志
/// </summary>
internal static partial class SecurityEventLog
{
    [LoggerMessage(EventId = 2601, Level = LogLevel.Warning, Message = "Security event queue is full, dropped {Type} ({Severity}): {Reason}")]
    public static partial void Dropped(ILogger logger, string type, short severity, string? reason);

    [LoggerMessage(EventId = 2602, Level = LogLevel.Warning, Message = "Writing {Count} security events failed, attempt {Attempt}")]
    public static partial void WriteFailed(ILogger logger, Exception exception, int count, int attempt);

    [LoggerMessage(EventId = 2603, Level = LogLevel.Error, Message = "Gave up writing {Count} security events after repeated failures")]
    public static partial void GaveUp(ILogger logger, int count);

    [LoggerMessage(EventId = 2604, Level = LogLevel.Error, Message = "Failed to record security event {Type}")]
    public static partial void RecordFailed(ILogger logger, Exception exception, string type);
}

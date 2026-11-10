using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>
/// LoggingBehavior 的日志方法
/// </summary>
internal static partial class LoggingBehaviorLog
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Message {MessageName} handled with {Outcome} in {ElapsedMs:0.0} ms (user {UserId}, role {RoleId})")]
    public static partial void MessageHandled(ILogger logger, string messageName, string outcome, double elapsedMs, Guid? userId, Guid? roleId);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "Slow message {MessageName} handled with {Outcome} in {ElapsedMs:0.0} ms (user {UserId}, role {RoleId})")]
    public static partial void SlowMessage(ILogger logger, string messageName, string outcome, double elapsedMs, Guid? userId, Guid? roleId);
}

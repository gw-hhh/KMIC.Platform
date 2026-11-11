using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 管道日志
/// </summary>
internal static partial class PipelineLog
{
    [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "After-commit action of {MessageName} failed")]
    public static partial void AfterCommitActionFailed(ILogger logger, Exception exception, string messageName);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning, Message = "Unique constraint {Schema}.{Constraint} violated by {MessageName} has no mapped error; declare MapsToError")]
    public static partial void UnmappedUniqueViolation(ILogger logger, string? schema, string? constraint, string messageName);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning, Message = "Commit outcome of {MessageName} is unknown")]
    public static partial void CommitOutcomeUnknown(ILogger logger, Exception exception, string messageName);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Error, Message = "Writing the read audit of {MessageName} failed, the query result was returned without an audit record")]
    public static partial void ReadAuditFailed(ILogger logger, Exception exception, string messageName);
}

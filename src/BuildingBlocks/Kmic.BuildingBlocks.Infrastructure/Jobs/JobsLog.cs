using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// 后台作业的日志
/// </summary>
internal static partial class JobsLog
{
    [LoggerMessage(EventId = 2301, Level = LogLevel.Warning, Message = "Background job {JobId} ({JobType}) failed with {ErrorCode}")]
    public static partial void JobFailed(ILogger logger, Exception? exception, Guid jobId, string jobType, string errorCode);

    [LoggerMessage(EventId = 2302, Level = LogLevel.Error, Message = "Commit outcome of a batch in background job {JobId} ({JobType}) is unknown, stopping without retry")]
    public static partial void BatchOutcomeUnknown(ILogger logger, Guid jobId, string jobType);

    [LoggerMessage(EventId = 2303, Level = LogLevel.Warning, Message = "Lease renewal of background job {JobId} failed")]
    public static partial void RenewFailed(ILogger logger, Exception exception, Guid jobId);
}

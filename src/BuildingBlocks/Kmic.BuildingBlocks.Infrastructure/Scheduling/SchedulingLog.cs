using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 定时任务的日志
/// </summary>
internal static partial class SchedulingLog
{
    [LoggerMessage(EventId = 2401, Level = LogLevel.Error, Message = "Scheduled job {JobName} failed with {ErrorCode}")]
    public static partial void RunFailed(ILogger logger, string jobName, string errorCode);

    [LoggerMessage(EventId = 2402, Level = LogLevel.Error, Message = "Commit outcome of scheduled job {JobName} is unknown, check before running it again")]
    public static partial void RunOutcomeUnknown(ILogger logger, string jobName);

    [LoggerMessage(EventId = 2403, Level = LogLevel.Error, Message = "Scheduled job {JobName} threw before its command finished")]
    public static partial void RunThrew(ILogger logger, Exception exception, string jobName);

    [LoggerMessage(EventId = 2404, Level = LogLevel.Warning, Message = "Scheduled job {JobName} fired but this instance does not declare it, skipped")]
    public static partial void UnknownJob(ILogger logger, string jobName);

    [LoggerMessage(EventId = 2405, Level = LogLevel.Warning, Message = "Override cron {Cron} of scheduled job {JobName} is invalid, using the default")]
    public static partial void InvalidOverride(ILogger logger, string jobName, string cron);

    [LoggerMessage(EventId = 2406, Level = LogLevel.Information, Message = "Scheduled job {JobName} was last synced by newer build {RecordedBuild}, left unchanged")]
    public static partial void NewerBuild(ILogger logger, string jobName, string recordedBuild);

    [LoggerMessage(EventId = 2407, Level = LogLevel.Information, Message = "Scheduled jobs synced: {Changed} changed, {Removed} removed")]
    public static partial void Synced(ILogger logger, int changed, int removed);
}

using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Observability;

internal static partial class ObservabilityLog
{
    [LoggerMessage(EventId = 2801, Level = LogLevel.Warning, Message = "Slow query in {Module}: {ElapsedMs:0} ms, failed {Failed}: {Sql}")]
    public static partial void SlowQuery(ILogger logger, string module, double elapsedMs, bool failed, string sql);

    [LoggerMessage(EventId = 2802, Level = LogLevel.Warning, Message = "Sampling the message and job backlog failed, the backlog gauges stop reporting until a sample succeeds")]
    public static partial void BacklogSampleFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2803, Level = LogLevel.Information, Message = "Backlog sampling started, every {IntervalSeconds:0} s for {Modules} modules")]
    public static partial void BacklogSamplingStarted(ILogger logger, double intervalSeconds, int modules);
}

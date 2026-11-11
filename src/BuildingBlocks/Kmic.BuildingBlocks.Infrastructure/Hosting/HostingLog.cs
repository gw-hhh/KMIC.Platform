using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>
/// 宿主与后台处理的日志
/// </summary>
internal static partial class HostingLog
{
    [LoggerMessage(EventId = 2201, Level = LogLevel.Information, Message = "Startup task {Name} completed in {ElapsedMs:0} ms")]
    public static partial void StartupTaskCompleted(ILogger logger, string name, double elapsedMs);
}

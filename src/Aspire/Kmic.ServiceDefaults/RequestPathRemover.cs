using Serilog.Core;
using Serilog.Events;

namespace Kmic.ServiceDefaults;

/// <summary>
/// 去掉所有日志里的请求实际路径，路径里可能带 ID 等业务数据
/// </summary>
internal sealed class RequestPathRemover : ILogEventEnricher
{
    private const string RequestPath = "RequestPath";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        logEvent.RemovePropertyIfPresent(RequestPath);
    }
}

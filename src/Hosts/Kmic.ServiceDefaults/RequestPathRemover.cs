using Serilog.Core;
using Serilog.Events;

namespace Kmic.ServiceDefaults;

// 移除 RequestPath，避免路径参数中的业务标识写入日志
internal sealed class RequestPathRemover : ILogEventEnricher
{
    private const string RequestPath = "RequestPath";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        logEvent.RemovePropertyIfPresent(RequestPath);
    }
}

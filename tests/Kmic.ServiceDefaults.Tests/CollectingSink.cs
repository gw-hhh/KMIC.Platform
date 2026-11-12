using Serilog.Core;
using Serilog.Events;

namespace Kmic.ServiceDefaults.Tests;

/// <summary>
/// 把日志事件收集在内存里
/// </summary>
internal sealed class CollectingSink : ILogEventSink
{
    private readonly List<LogEvent> _events = [];

    public IReadOnlyList<LogEvent> Events
    {
        get
        {
            lock (_events)
            {
                return [.. _events];
            }
        }
    }

    public void Emit(LogEvent logEvent)
    {
        lock (_events)
        {
            _events.Add(logEvent);
        }
    }
}

using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Application.Security;

namespace Kmic.BuildingBlocks.Testing.Security;

/// <summary>记录安全事件，供测试断言。</summary>
public sealed class RecordingSecurityEventWriter : ISecurityEventWriter
{
    private readonly ConcurrentQueue<SecurityEvent> _events = new();

    /// <summary>按写入顺序返回已记录的安全事件。</summary>
    public IReadOnlyCollection<SecurityEvent> Events => _events;

    public void Write(SecurityEvent securityEvent)
    {
        ArgumentNullException.ThrowIfNull(securityEvent);
        _events.Enqueue(securityEvent);
    }
}

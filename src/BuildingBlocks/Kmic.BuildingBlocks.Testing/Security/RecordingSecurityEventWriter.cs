using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Application.Security;

namespace Kmic.BuildingBlocks.Testing.Security;

/// <summary>
/// 记下写入的安全事件，供断言；阶段 3 之前也代替 Identity 的实现
/// </summary>
public sealed class RecordingSecurityEventWriter : ISecurityEventWriter
{
    private readonly ConcurrentQueue<SecurityEvent> _events = new();

    /// <summary>
    /// 已写入的事件，按写入顺序
    /// </summary>
    public IReadOnlyCollection<SecurityEvent> Events => _events;

    /// <inheritdoc />
    public void Write(SecurityEvent securityEvent)
    {
        ArgumentNullException.ThrowIfNull(securityEvent);
        _events.Enqueue(securityEvent);
    }
}

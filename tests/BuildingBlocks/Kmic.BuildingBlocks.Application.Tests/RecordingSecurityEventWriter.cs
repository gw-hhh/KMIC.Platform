using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Application.Security;

namespace Kmic.BuildingBlocks.Application.Tests;

internal sealed class RecordingSecurityEventWriter : ISecurityEventWriter
{
    private readonly ConcurrentQueue<SecurityEvent> _events = new();

    public IReadOnlyCollection<SecurityEvent> Events => _events;

    public void Write(SecurityEvent securityEvent) => _events.Enqueue(securityEvent);
}

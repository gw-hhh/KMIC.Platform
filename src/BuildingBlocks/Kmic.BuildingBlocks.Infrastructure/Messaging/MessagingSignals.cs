using System.Collections.Concurrent;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 按频道和模块分开的唤醒信号
/// </summary>
internal sealed class MessagingSignals
{
    private readonly ConcurrentDictionary<(string Channel, string Module), AsyncSignal> _signals = new();

    public AsyncSignal For(string channel, string module) => _signals.GetOrAdd((channel, module), _ => new AsyncSignal());

    public void Wake(string channel, string module)
    {
        if (_signals.TryGetValue((channel, module), out var signal))
        {
            signal.Set();
        }
    }

    public void WakeAll()
    {
        foreach (var signal in _signals.Values)
        {
            signal.Set();
        }
    }
}

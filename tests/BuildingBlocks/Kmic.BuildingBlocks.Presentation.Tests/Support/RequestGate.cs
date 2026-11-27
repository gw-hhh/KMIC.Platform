using System.Threading.Channels;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

// 将请求阻塞在处理器内直到测试放行，用于构造并发处理场景
internal sealed class RequestGate
{
    private static readonly TimeSpan _patience = TimeSpan.FromSeconds(10);

    private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Channel<bool> _entered = Channel.CreateUnbounded<bool>();

    public async Task PassAsync(CancellationToken ct)
    {
        _entered.Writer.TryWrite(true);
        await _release.Task.WaitAsync(_patience, ct);
    }

    public async Task WaitEnteredAsync(int count, CancellationToken ct)
    {
        for (var i = 0; i < count; i++)
        {
            await _entered.Reader.ReadAsync(ct).AsTask().WaitAsync(_patience, ct);
        }
    }

    public void Release() => _release.TrySetResult();
}

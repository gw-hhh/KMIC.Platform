using System.Threading.Channels;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

/// <summary>
/// 让请求停在处理器里，直到测试放行：用来制造“同时有 N 个请求在处理”
/// </summary>
internal sealed class RequestGate
{
    private static readonly TimeSpan _patience = TimeSpan.FromSeconds(10);

    private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Channel<bool> _entered = Channel.CreateUnbounded<bool>();

    /// <summary>
    /// 处理器调用：报告已进入，然后等放行
    /// </summary>
    public async Task PassAsync(CancellationToken ct)
    {
        _entered.Writer.TryWrite(true);
        await _release.Task.WaitAsync(_patience, ct);
    }

    /// <summary>
    /// 测试调用：等到有这么多个请求进入了处理器
    /// </summary>
    public async Task WaitEnteredAsync(int count, CancellationToken ct)
    {
        for (var i = 0; i < count; i++)
        {
            await _entered.Reader.ReadAsync(ct).AsTask().WaitAsync(_patience, ct);
        }
    }

    /// <summary>
    /// 放行所有停住的请求，之后进来的也直接通过
    /// </summary>
    public void Release() => _release.TrySetResult();
}

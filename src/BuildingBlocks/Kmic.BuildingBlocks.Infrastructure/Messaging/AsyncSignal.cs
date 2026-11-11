namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 唤醒一个等待中的循环
/// </summary>
/// <remarks>
/// 等待开始前就收到的唤醒不会丢：那次等待会立刻返回
/// </remarks>
internal sealed class AsyncSignal
{
    private TaskCompletionSource _pending = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Set() => Volatile.Read(ref _pending).TrySetResult();

    /// <summary>
    /// 等到被唤醒、超时或取消，都正常返回；被唤醒时返回 true
    /// </summary>
    public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken ct)
    {
        var pending = Volatile.Read(ref _pending);
        var woken = false;
        try
        {
            await pending.Task.WaitAsync(timeout, ct);
            woken = true;
        }
        catch (TimeoutException)
        {
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }

        // 换一个新的等待。换之前到达的唤醒落在旧的上面，调用方接下来本来就要查一次，不会漏掉数据
        Interlocked.CompareExchange(ref _pending, new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously), pending);
        return woken;
    }
}

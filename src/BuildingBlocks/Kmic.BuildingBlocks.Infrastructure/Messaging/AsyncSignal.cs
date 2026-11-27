namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 异步等待循环唤醒信号（WaitAsync 前收到的 Set 不会丢失）
internal sealed class AsyncSignal
{
    private TaskCompletionSource _pending = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Set() => Volatile.Read(ref _pending).TrySetResult();

    // 被信号唤醒返回 true，超时或取消返回 false
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

        // 替换为新的等待句柄；替换前到达的信号作用于旧句柄，且调用方随后会执行一次查询，不会遗漏数据
        Interlocked.CompareExchange(ref _pending, new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously), pending);
        return woken;
    }
}

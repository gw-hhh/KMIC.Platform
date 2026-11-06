namespace Spike15.Tests;

// 让 N 个并发命令都占住事务连接之后再一起发起嵌套读取，制造最坏的时序
internal sealed class AsyncBarrier(int participants)
{
    private readonly TaskCompletionSource _allArrived = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _arrived;

    public Task SignalAndWaitAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Increment(ref _arrived) == participants)
        {
            _allArrived.SetResult();
        }

        return _allArrived.Task.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
    }
}

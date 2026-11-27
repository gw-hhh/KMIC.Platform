namespace Kmic.BuildingBlocks.Testing.Waiting;

/// <summary>异步轮询等待辅助工具（按系统真实时间轮询，不受宿主内 FakeTimeProvider 影响）。</summary>
public static class Eventually
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan _interval = TimeSpan.FromMilliseconds(50);

    /// <summary>周期性探测直至结果满足条件并返回最终结果，超时时抛出 <see cref="TimeoutException" />。</summary>
    public static async Task<T> UntilAsync<T>(
        Func<CancellationToken, Task<T>> probe,
        Func<T, bool> done,
        string because,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(probe);
        ArgumentNullException.ThrowIfNull(done);

        var limit = timeout ?? DefaultTimeout;
        var started = TimeProvider.System.GetTimestamp();
        while (true)
        {
            var current = await probe(ct);
            if (done(current))
            {
                return current;
            }

            if (TimeProvider.System.GetElapsedTime(started) >= limit)
            {
                throw new TimeoutException($"等待 {limit.TotalSeconds:0.#} 秒仍未完成：{because}。最后一次读取到：{current}");
            }

            await Task.Delay(_interval, TimeProvider.System, ct);
        }
    }

    /// <summary>周期性检查直至条件成立，超时时抛出 <see cref="TimeoutException" />。</summary>
    public static Task UntilAsync(
        Func<CancellationToken, Task<bool>> condition,
        string because,
        TimeSpan? timeout = null,
        CancellationToken ct = default) =>
        UntilAsync(condition, satisfied => satisfied, because, timeout, ct);
}

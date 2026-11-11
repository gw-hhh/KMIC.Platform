namespace Kmic.BuildingBlocks.Testing.Waiting;

/// <summary>
/// 等待后台处理完成
/// </summary>
/// <remarks>
/// 测试里不能直接 Task.Delay 或 Thread.Sleep，等后台处理要用它。按真实时间轮询，和宿主里的 FakeTimeProvider 无关
/// </remarks>
public static class Eventually
{
    /// <summary>
    /// 默认超时
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan _interval = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// 反复读取，直到结果满足条件，返回那次的结果
    /// </summary>
    /// <param name="probe">读取当前状态</param>
    /// <param name="done">是否已完成</param>
    /// <param name="because">在等什么，超时时显示</param>
    /// <param name="timeout">超时时间，默认 10 秒</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="TimeoutException">超时仍未满足</exception>
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

    /// <summary>
    /// 反复检查，直到条件成立
    /// </summary>
    /// <exception cref="TimeoutException">超时仍未成立</exception>
    public static Task UntilAsync(
        Func<CancellationToken, Task<bool>> condition,
        string because,
        TimeSpan? timeout = null,
        CancellationToken ct = default) =>
        UntilAsync(condition, satisfied => satisfied, because, timeout, ct);
}

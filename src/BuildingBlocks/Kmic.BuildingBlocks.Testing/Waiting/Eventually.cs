namespace Kmic.BuildingBlocks.Testing.Waiting;

/// <summary>
/// 等待后台处理（Outbox 投递、后台作业等）完成：按间隔轮询，超时即失败
/// </summary>
/// <remarks>
/// 测试项目禁止直接 Task.Delay / Thread.Sleep（06 第 12.3 节）；这里按真实时间等待，与宿主中的 FakeTimeProvider 无关
/// </remarks>
public static class Eventually
{
    /// <summary>
    /// 默认超时
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan _interval = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// 反复读取，直到结果满足条件；返回满足条件的那次结果
    /// </summary>
    /// <param name="probe">读取当前状态</param>
    /// <param name="done">是否已完成</param>
    /// <param name="because">超时时的说明，写明在等什么</param>
    /// <param name="timeout">超时，默认 10 秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <exception cref="TimeoutException">超时仍未满足</exception>
    public static async Task<T> UntilAsync<T>(
        Func<CancellationToken, Task<T>> probe,
        Func<T, bool> done,
        string because,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(probe);
        ArgumentNullException.ThrowIfNull(done);

        var limit = timeout ?? DefaultTimeout;
        var started = TimeProvider.System.GetTimestamp();
        while (true)
        {
            var current = await probe(cancellationToken);
            if (done(current))
            {
                return current;
            }

            if (TimeProvider.System.GetElapsedTime(started) >= limit)
            {
                throw new TimeoutException($"等待 {limit.TotalSeconds:0.#} 秒仍未完成：{because}。最后一次读取到：{current}");
            }

            await Task.Delay(_interval, TimeProvider.System, cancellationToken);
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
        CancellationToken cancellationToken = default) =>
        UntilAsync(condition, satisfied => satisfied, because, timeout, cancellationToken);
}

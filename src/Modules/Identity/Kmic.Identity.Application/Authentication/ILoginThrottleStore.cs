namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 登录失败计数的存储
/// </summary>
/// <remarks>
/// 要用独立事务提交，不随登录回滚
/// </remarks>
public interface ILoginThrottleStore
{
    /// <summary>
    /// 当前状态，没有失败记录时为空
    /// </summary>
    Task<LoginThrottleState?> GetAsync(string loginNameHash, CancellationToken ct);

    /// <summary>
    /// 记录登录失败；加行锁后更新状态，避免并发计数丢失
    /// </summary>
    Task<LoginThrottleState> RecordFailureAsync(string loginNameHash, DateTimeOffset now, CancellationToken ct);

    /// <summary>
    /// 清除指定登录名的失败计数和登录限制
    /// </summary>
    Task ClearAsync(string loginNameHash, CancellationToken ct);

    /// <summary>
    /// 删除最近失败时间早于 <paramref name="lastFailedBefore" /> 且当前不在等待或锁定中的记录，返回删除条数
    /// </summary>
    Task<int> PurgeAsync(DateTimeOffset lastFailedBefore, DateTimeOffset now, CancellationToken ct);
}

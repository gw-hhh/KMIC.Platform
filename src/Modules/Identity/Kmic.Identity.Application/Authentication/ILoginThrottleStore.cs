namespace Kmic.Identity.Application.Authentication;

/// <summary>登录失败计数存储（须独立提交，不随登录事务回滚）</summary>
public interface ILoginThrottleStore
{
    /// <summary>获取当前限流状态；无记录时返回 null</summary>
    Task<LoginThrottleState?> GetAsync(string loginNameHash, CancellationToken ct);

    /// <summary>加行锁记录登录失败，避免并发计数丢失</summary>
    Task<LoginThrottleState> RecordFailureAsync(string loginNameHash, DateTimeOffset now, CancellationToken ct);

    /// <summary>清除指定登录名的失败计数与限制</summary>
    Task ClearAsync(string loginNameHash, CancellationToken ct);

    /// <summary>清理最近失败早于 <paramref name="lastFailedBefore" /> 且当前未限流或锁定的记录，返回删除条数</summary>
    Task<int> PurgeAsync(DateTimeOffset lastFailedBefore, DateTimeOffset now, CancellationToken ct);
}

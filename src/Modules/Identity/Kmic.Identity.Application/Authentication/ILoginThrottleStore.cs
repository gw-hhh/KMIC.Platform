namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 登录失败计数的存储
/// </summary>
/// <remarks>
/// 失败计数必须在登录命令回滚后仍然保留，所以实现用独立事务提交，不走命令事务。
/// 键是规范化登录名的 SHA-256，不存不存在账号的明文
/// </remarks>
public interface ILoginThrottleStore
{
    /// <summary>
    /// 当前状态，没有失败记录时为空
    /// </summary>
    Task<LoginThrottleState?> GetAsync(string loginNameHash, CancellationToken ct);

    /// <summary>
    /// 记一次失败：行上加锁后按规则算出新状态并保存，并发的失败不会丢计数
    /// </summary>
    Task<LoginThrottleState> RecordFailureAsync(string loginNameHash, DateTimeOffset now, CancellationToken ct);

    /// <summary>
    /// 清零：登录成功、管理员解锁或用短信重置密码之后
    /// </summary>
    Task ClearAsync(string loginNameHash, CancellationToken ct);

    /// <summary>
    /// 删除不再有意义的行：最近一次失败早于 <paramref name="lastFailedBefore" />，而且当前没有处在等待或锁定中，返回删除的行数
    /// </summary>
    Task<int> PurgeAsync(DateTimeOffset lastFailedBefore, DateTimeOffset now, CancellationToken ct);
}

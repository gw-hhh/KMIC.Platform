namespace Kmic.Identity.Infrastructure.Persistence.Records;

/// <summary>
/// 登录失败计数表的一行：按规范化登录名的哈希，不经过聚合，用原子 SQL 更新
/// </summary>
internal sealed class LoginThrottleRow
{
    /// <summary>
    /// 规范化登录名的 SHA-256，小写十六进制
    /// </summary>
    public required string LoginNameHash { get; init; }

    /// <summary>
    /// 连续失败次数
    /// </summary>
    public int FailureCount { get; init; }

    /// <summary>
    /// 已经锁定过几次
    /// </summary>
    public int LockLevel { get; init; }

    /// <summary>
    /// 在此之前不能再试
    /// </summary>
    public DateTimeOffset? NextAllowedAt { get; init; }

    /// <summary>
    /// 锁定到何时
    /// </summary>
    public DateTimeOffset? LockedUntil { get; init; }

    /// <summary>
    /// 最后一次失败的时间
    /// </summary>
    public DateTimeOffset LastFailedAt { get; init; }
}

namespace Kmic.Identity.Application.Authentication;

/// <summary>登录名对应的失败计数与锁定状态</summary>
/// <param name="FailureCount">连续失败次数</param>
/// <param name="LockLevel">累计锁定次数</param>
/// <param name="NextAllowedAt">允许下次尝试的最早时间，无限流等待时为空</param>
/// <param name="LockedUntil">锁定截止时间，未锁定时为空</param>
/// <param name="LastFailedAt">最近失败时间</param>
public sealed record LoginThrottleState(int FailureCount, int LockLevel, DateTimeOffset? NextAllowedAt, DateTimeOffset? LockedUntil, DateTimeOffset LastFailedAt);

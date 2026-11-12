namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 一个登录名的失败计数状态
/// </summary>
/// <param name="FailureCount">连续失败次数</param>
/// <param name="LockLevel">已经锁定过几次</param>
/// <param name="NextAllowedAt">在此之前不能再试；没有等待时为空</param>
/// <param name="LockedUntil">锁定到何时；没有锁定时为空</param>
/// <param name="LastFailedAt">最后一次失败的时间</param>
public sealed record LoginThrottleState(int FailureCount, int LockLevel, DateTimeOffset? NextAllowedAt, DateTimeOffset? LockedUntil, DateTimeOffset LastFailedAt);

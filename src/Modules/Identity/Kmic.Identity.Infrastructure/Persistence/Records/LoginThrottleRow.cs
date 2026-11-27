namespace Kmic.Identity.Infrastructure.Persistence.Records;

internal sealed class LoginThrottleRow
{
    // 规范化登录名的 SHA-256（小写十六进制）
    public required string LoginNameHash { get; init; }

    public int FailureCount { get; init; }

    public int LockLevel { get; init; }

    public DateTimeOffset? NextAllowedAt { get; init; }

    public DateTimeOffset? LockedUntil { get; init; }

    public DateTimeOffset LastFailedAt { get; init; }
}

using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 登录失败的等待与锁定算法
/// </summary>
public sealed class LoginThrottlePolicy(IOptions<LoginThrottleOptions> settings)
{
    private LoginThrottleOptions Options => settings.Value;

    /// <summary>
    /// 距离下次允许登录的等待时间；无需等待时返回 null
    /// </summary>
    public static TimeSpan? Remaining(LoginThrottleState? state, DateTimeOffset now)
    {
        if (state is null)
        {
            return null;
        }

        var until = Later(state.NextAllowedAt, state.LockedUntil);
        return until is { } moment && moment > now ? moment - now : null;
    }

    /// <summary>
    /// 计算本次登录失败后的状态
    /// </summary>
    public LoginThrottleState OnFailure(LoginThrottleState? previous, DateTimeOffset now)
    {
        // 超过重置间隔时，失败次数和锁定等级一并重置
        var current = previous is not null && now - previous.LastFailedAt <= Options.ResetAfter ? previous : null;
        var count = (current?.FailureCount ?? 0) + 1;
        var level = current?.LockLevel ?? 0;

        if (count < Options.LockAfterFailures)
        {
            return new LoginThrottleState(count, level, WaitUntil(count, now), null, now);
        }

        var sinceFirstLock = count - Options.LockAfterFailures;
        if (sinceFirstLock % Options.RelockEveryFailures != 0)
        {
            // 两次锁定之间的失败：沿用最后一档等待
            return new LoginThrottleState(count, level, now + Options.Waits[^1], null, now);
        }

        var lockLevel = level + 1;
        return new LoginThrottleState(count, lockLevel, null, now + LockDuration(lockLevel), now);
    }

    private TimeSpan LockDuration(int lockLevel)
    {
        var duration = Options.FirstLock;
        for (var i = 1; i < lockLevel && duration < Options.MaxLock; i++)
        {
            duration *= 2;
        }

        return duration < Options.MaxLock ? duration : Options.MaxLock;
    }

    private DateTimeOffset? WaitUntil(int count, DateTimeOffset now)
    {
        var index = count - Options.FreeFailures - 1;
        return index < 0 ? null : now + Options.Waits[Math.Min(index, Options.Waits.Count - 1)];
    }

    private static DateTimeOffset? Later(DateTimeOffset? a, DateTimeOffset? b) => (a, b) switch
    {
        (null, null) => null,
        (null, _) => b,
        (_, null) => a,
        _ => a > b ? a : b,
    };
}

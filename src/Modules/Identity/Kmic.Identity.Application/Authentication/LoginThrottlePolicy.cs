namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 登录失败的等待与锁定算法
/// </summary>
/// <remarks>
/// 纯计算，不碰数据库：状态怎样原子地存取由存储层负责。计数按规范化后的登录名，
/// 不存在的登录名同样计数、同样等待，所以不会暴露账号是否存在
/// </remarks>
/// <param name="options">规则</param>
public sealed class LoginThrottlePolicy(LoginThrottleOptions options)
{
    /// <summary>
    /// 现在还要等多久才能尝试；可以尝试时为空
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
    /// 又失败了一次之后的新状态
    /// </summary>
    public LoginThrottleState OnFailure(LoginThrottleState? previous, DateTimeOffset now)
    {
        // 很久没有失败，从头开始数
        var current = previous is not null && now - previous.LastFailedAt <= options.ResetAfter ? previous : null;
        var count = (current?.FailureCount ?? 0) + 1;
        var level = current?.LockLevel ?? 0;

        if (count < options.LockAfterFailures)
        {
            return new LoginThrottleState(count, level, WaitUntil(count, now), null, now);
        }

        var sinceFirstLock = count - options.LockAfterFailures;
        if (sinceFirstLock % options.RelockEveryFailures != 0)
        {
            // 两次锁定之间的失败：沿用最后一档等待
            return new LoginThrottleState(count, level, now + options.Waits[^1], null, now);
        }

        var lockLevel = level + 1;
        return new LoginThrottleState(count, lockLevel, null, now + LockDuration(lockLevel), now);
    }

    private TimeSpan LockDuration(int lockLevel)
    {
        var duration = options.FirstLock;
        for (var i = 1; i < lockLevel && duration < options.MaxLock; i++)
        {
            duration *= 2;
        }

        return duration < options.MaxLock ? duration : options.MaxLock;
    }

    private DateTimeOffset? WaitUntil(int count, DateTimeOffset now)
    {
        var index = count - options.FreeFailures - 1;
        return index < 0 ? null : now + options.Waits[Math.Min(index, options.Waits.Count - 1)];
    }

    private static DateTimeOffset? Later(DateTimeOffset? a, DateTimeOffset? b) => (a, b) switch
    {
        (null, null) => null,
        (null, _) => b,
        (_, null) => a,
        _ => a > b ? a : b,
    };
}

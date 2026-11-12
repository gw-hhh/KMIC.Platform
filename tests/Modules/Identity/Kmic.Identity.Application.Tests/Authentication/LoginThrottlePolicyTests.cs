using Kmic.Identity.Application.Authentication;
using Shouldly;

namespace Kmic.Identity.Application.Tests.Authentication;

public sealed class LoginThrottlePolicyTests
{
    private static readonly DateTimeOffset _t0 = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    private readonly LoginThrottlePolicy _policy = new(new LoginThrottleOptions());

    // 在第 n 次失败时没有等待，等待从上一次失败之后起算：测试里每次失败都发生在等待结束之后
    private LoginThrottleState FailTimes(int times, DateTimeOffset start, out DateTimeOffset now)
    {
        LoginThrottleState? state = null;
        now = start;
        for (var i = 0; i < times; i++)
        {
            state = _policy.OnFailure(state, now);
            if (LoginThrottlePolicy.Remaining(state, now) is { } wait)
            {
                now += wait;
            }
        }

        return state!;
    }

    [Fact]
    public void NoState_NothingToWaitFor() => LoginThrottlePolicy.Remaining(null, _t0).ShouldBeNull();

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 30)]
    [InlineData(4, 60)]
    [InlineData(5, 120)]
    [InlineData(6, 300)]
    [InlineData(9, 300)]
    public void WaitAfterTheNthFailure_FollowsTheSchedule(int failures, int expectedSeconds)
    {
        var state = FailTimes(failures, _t0, out _);
        var justFailed = state.LastFailedAt;

        var remaining = LoginThrottlePolicy.Remaining(state, justFailed);

        if (expectedSeconds == 0)
        {
            remaining.ShouldBeNull();
        }
        else
        {
            remaining.ShouldBe(TimeSpan.FromSeconds(expectedSeconds));
        }

        state.FailureCount.ShouldBe(failures);
        state.LockedUntil.ShouldBeNull();
    }

    [Fact]
    public void TenthFailure_LocksForThirtyMinutes_AndClearsTheShortWait()
    {
        var state = FailTimes(10, _t0, out _);

        state.LockedUntil.ShouldBe(state.LastFailedAt.AddMinutes(30));
        state.NextAllowedAt.ShouldBeNull();
        state.LockLevel.ShouldBe(1);
        LoginThrottlePolicy.Remaining(state, state.LastFailedAt).ShouldBe(TimeSpan.FromMinutes(30));
        LoginThrottlePolicy.Remaining(state, state.LastFailedAt.AddMinutes(30)).ShouldBeNull();
    }

    [Fact]
    public void FailuresBetweenLocks_KeepTheLastWaitTier_AndEveryFifthRelocksWithDoubleTheTime()
    {
        var state = FailTimes(11, _t0, out _);
        state.LockedUntil.ShouldBeNull();
        LoginThrottlePolicy.Remaining(state, state.LastFailedAt).ShouldBe(TimeSpan.FromMinutes(5));

        var fifteenth = FailTimes(15, _t0, out _);

        fifteenth.LockLevel.ShouldBe(2);
        fifteenth.LockedUntil.ShouldBe(fifteenth.LastFailedAt.AddHours(1));
        FailTimes(20, _t0, out _).LockedUntil.ShouldBe(FailTimes(20, _t0, out _).LastFailedAt.AddHours(2));
    }

    [Fact]
    public void RepeatedLocks_NeverExceedTheMaximum()
    {
        var state = FailTimes(10 + (5 * 12), _t0, out _);

        state.LockLevel.ShouldBe(13);
        state.LockedUntil.ShouldBe(state.LastFailedAt.AddHours(24));
    }

    [Fact]
    public void LongAfterTheLastFailure_TheCountStartsOver()
    {
        var state = FailTimes(5, _t0, out var now);

        var next = _policy.OnFailure(state, now.AddHours(25));

        next.FailureCount.ShouldBe(1);
        next.LockLevel.ShouldBe(0);
        next.NextAllowedAt.ShouldBeNull();
    }

    [Fact]
    public void CustomOptions_AreHonored()
    {
        var strict = new LoginThrottlePolicy(new LoginThrottleOptions { FreeFailures = 0, Waits = [TimeSpan.FromSeconds(10)], LockAfterFailures = 3, FirstLock = TimeSpan.FromMinutes(1), RelockEveryFailures = 2, MaxLock = TimeSpan.FromMinutes(3) });

        var first = strict.OnFailure(null, _t0);
        var second = strict.OnFailure(first, _t0.AddSeconds(10));
        var third = strict.OnFailure(second, _t0.AddSeconds(20));
        var fifth = strict.OnFailure(strict.OnFailure(third, _t0.AddMinutes(2)), _t0.AddMinutes(3));

        LoginThrottlePolicy.Remaining(first, _t0).ShouldBe(TimeSpan.FromSeconds(10));
        third.LockedUntil.ShouldBe(_t0.AddSeconds(20).AddMinutes(1));
        fifth.LockLevel.ShouldBe(2);
        fifth.LockedUntil.ShouldBe(_t0.AddMinutes(3).AddMinutes(2));
    }
}

namespace Kmic.Identity.Application.Authentication;

/// <summary>登录失败等待与锁定策略配置</summary>
public sealed class LoginThrottleOptions
{
    public const string Section = "Identity:LoginThrottle";

    /// <summary>免限流失败次数</summary>
    public int FreeFailures { get; init; } = 2;

    /// <summary>逐次退避时长（超出免限流次数后生效）</summary>
    public IReadOnlyList<TimeSpan> Waits { get; init; } = [TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(5)];

    /// <summary>触发锁定的连续失败次数</summary>
    public int LockAfterFailures { get; init; } = 10;

    /// <summary>首次锁定时长</summary>
    public TimeSpan FirstLock { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>再次锁定阈值（解锁后触发锁定的新增失败次数）</summary>
    public int RelockEveryFailures { get; init; } = 5;

    /// <summary>重复锁定上限（每次翻倍）</summary>
    public TimeSpan MaxLock { get; init; } = TimeSpan.FromHours(24);

    /// <summary>计数保留时长（无新增失败超期后重置）</summary>
    public TimeSpan ResetAfter { get; init; } = TimeSpan.FromHours(24);
}

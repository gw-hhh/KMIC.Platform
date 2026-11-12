namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 登录失败后的等待与锁定规则
/// </summary>
/// <remarks>
/// 部署配置，节名 Identity:LoginThrottle。安全类阈值放在配置里而不是系统设置：改动要发布并留痕，不在界面上调
/// </remarks>
public sealed class LoginThrottleOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Identity:LoginThrottle";

    /// <summary>
    /// 前几次失败不限制
    /// </summary>
    public int FreeFailures { get; init; } = 2;

    /// <summary>
    /// 不限制的失败之后，每次失败要等多久才能再试，依次对应第 FreeFailures + 1 次、之后每一次；用完后沿用最后一项，直到锁定
    /// </summary>
    public IReadOnlyList<TimeSpan> Waits { get; init; } = [TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(5)];

    /// <summary>
    /// 连续失败到第几次时锁定
    /// </summary>
    public int LockAfterFailures { get; init; } = 10;

    /// <summary>
    /// 第一次锁定的时长
    /// </summary>
    public TimeSpan FirstLock { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// 锁定解除后，每再失败多少次重新锁定
    /// </summary>
    public int RelockEveryFailures { get; init; } = 5;

    /// <summary>
    /// 重新锁定的时长上限，每次重新锁定时长翻倍
    /// </summary>
    public TimeSpan MaxLock { get; init; } = TimeSpan.FromHours(24);

    /// <summary>
    /// 多久没有新的失败，计数就清零
    /// </summary>
    public TimeSpan ResetAfter { get; init; } = TimeSpan.FromHours(24);
}

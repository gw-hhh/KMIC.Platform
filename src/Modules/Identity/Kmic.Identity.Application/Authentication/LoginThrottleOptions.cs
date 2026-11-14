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
    /// 开始限流前允许的失败次数
    /// </summary>
    public int FreeFailures { get; init; } = 2;

    /// <summary>
    /// 超过 FreeFailures 后的逐次等待时长，依次对应后续每次失败；列表用完后沿用最后一项，直至锁定
    /// </summary>
    public IReadOnlyList<TimeSpan> Waits { get; init; } = [TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(5)];

    /// <summary>
    /// 触发锁定的连续失败次数
    /// </summary>
    public int LockAfterFailures { get; init; } = 10;

    /// <summary>
    /// 第一次锁定的时长
    /// </summary>
    public TimeSpan FirstLock { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// 解锁后再次触发锁定所需的新增失败次数
    /// </summary>
    public int RelockEveryFailures { get; init; } = 5;

    /// <summary>
    /// 重新锁定的时长上限，每次重新锁定时长翻倍
    /// </summary>
    public TimeSpan MaxLock { get; init; } = TimeSpan.FromHours(24);

    /// <summary>
    /// 无新增失败时的计数保留时长，到期后重置
    /// </summary>
    public TimeSpan ResetAfter { get; init; } = TimeSpan.FromHours(24);
}

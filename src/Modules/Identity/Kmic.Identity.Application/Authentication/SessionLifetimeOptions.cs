using Kmic.BuildingBlocks.Application.Time;

namespace Kmic.Identity.Application.Authentication;

/// <summary>会话与刷新令牌有效期配置</summary>
public sealed class SessionLifetimeOptions
{
    public const string Section = "Identity:Session";

    /// <summary>网页刷新令牌的滑动有效期</summary>
    [TimeSpanRange("00:10:00", "90.00:00:00")]
    public TimeSpan WebRefreshLifetime { get; init; } = TimeSpan.FromDays(7);

    /// <summary>移动端刷新令牌的滑动有效期</summary>
    [TimeSpanRange("00:10:00", "180.00:00:00")]
    public TimeSpan AppRefreshLifetime { get; init; } = TimeSpan.FromDays(30);

    /// <summary>网页会话绝对有效期上限，到期后须重新登录</summary>
    [TimeSpanRange("00:10:00", "365.00:00:00")]
    public TimeSpan WebAbsoluteLifetime { get; init; } = TimeSpan.FromDays(30);

    /// <summary>移动端会话绝对有效期上限</summary>
    [TimeSpanRange("00:10:00", "365.00:00:00")]
    public TimeSpan AppAbsoluteLifetime { get; init; } = TimeSpan.FromDays(90);

    /// <summary>并发刷新宽限期，已轮换且未过期的旧令牌在此期间再次使用时按并发刷新处理</summary>
    [TimeSpanRange("00:00:00", "00:01:00")]
    public TimeSpan RefreshGrace { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>模拟会话有效期</summary>
    [TimeSpanRange("00:05:00", "04:00:00")]
    public TimeSpan ImpersonationLifetime { get; init; } = TimeSpan.FromHours(1);

    /// <summary>会话结束后的保留时长，超期后由定时任务清理</summary>
    [TimeSpanRange("1.00:00:00", "365.00:00:00")]
    public TimeSpan EndedRetention { get; init; } = TimeSpan.FromDays(30);
}

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 允许携带刷新 Cookie 访问认证端点的来源
/// </summary>
/// <remarks>
/// 配置节为 Identity:Cookie。携带刷新 Cookie 的刷新请求须同时满足可信 Origin 和 X-Requested-With: kmic；退出及切换角色使用 Bearer 认证
/// </remarks>
public sealed class TrustedOriginsOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Identity:Cookie";

    /// <summary>
    /// 可信来源，如 https://app.example.com，不带末尾斜杠
    /// </summary>
    public IReadOnlyList<string> TrustedOrigins { get; init; } = [];
}

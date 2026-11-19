namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 允许携带刷新 Cookie 访问认证端点的来源，配置节 Identity:Cookie
/// </summary>
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

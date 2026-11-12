namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 允许携带刷新 Cookie 访问认证端点的来源
/// </summary>
/// <remarks>
/// 部署配置，节名 Identity:Cookie。刷新、退出、切换角色这些端点要同时满足：Origin 在名单里、带自定义请求头 X-Requested-With: kmic
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

namespace Kmic.Identity.Application.Authentication;

/// <summary>允许携带刷新 Cookie 访问认证端点的可信来源配置</summary>
public sealed class TrustedOriginsOptions
{
    public const string Section = "Identity:Cookie";

    /// <summary>可信来源地址（如 https://app.example.com，不含末尾斜杠）</summary>
    public IReadOnlyList<string> TrustedOrigins { get; init; } = [];
}

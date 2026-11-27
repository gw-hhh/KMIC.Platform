using System.Text.Json.Serialization;

namespace Kmic.Identity.Application.Authentication;

/// <summary>认证令牌响应（网页端刷新令牌经 Cookie 传递，响应体中为空）</summary>
/// <param name="AccessToken">访问令牌</param>
/// <param name="TokenType">固定为 Bearer</param>
/// <param name="ExpiresIn">访问令牌有效期（秒）</param>
/// <param name="RefreshToken">刷新令牌，未签发新令牌时为空</param>
/// <param name="PasswordChangeRequired">是否必须先修改密码</param>
public sealed record AuthTokens(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("passwordChangeRequired")] bool PasswordChangeRequired)
{
    // 禁用编译器生成的 ToString 实现，避免在日志中输出令牌明文
    public override string ToString() => nameof(AuthTokens);
}

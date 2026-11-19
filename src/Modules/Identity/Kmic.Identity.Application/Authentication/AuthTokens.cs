using System.Text.Json.Serialization;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 认证令牌响应
/// </summary>
/// <remarks>
/// 网页端的刷新令牌在 Cookie 里，这里为空
/// </remarks>
/// <param name="AccessToken">访问令牌</param>
/// <param name="TokenType">固定为 Bearer</param>
/// <param name="ExpiresIn">访问令牌有效期（秒）</param>
/// <param name="RefreshToken">刷新令牌，没有新签发时为空</param>
/// <param name="PasswordChangeRequired">是否必须先修改密码</param>
public sealed record AuthTokens(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("passwordChangeRequired")] bool PasswordChangeRequired);

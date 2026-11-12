using System.Text.Json.Serialization;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 登录、刷新、切换角色返回的令牌
/// </summary>
/// <remarks>
/// 字段名沿用 OAuth2。网页客户端的刷新令牌放在 Cookie 里，接口层会把响应体里的 refresh_token 去掉
/// </remarks>
/// <param name="AccessToken">访问令牌</param>
/// <param name="TokenType">固定为 Bearer</param>
/// <param name="ExpiresIn">访问令牌还有多少秒过期</param>
/// <param name="RefreshToken">刷新令牌；没有新签发时为空</param>
/// <param name="PasswordChangeRequired">是否必须先修改密码</param>
public sealed record AuthTokens(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("passwordChangeRequired")] bool PasswordChangeRequired);

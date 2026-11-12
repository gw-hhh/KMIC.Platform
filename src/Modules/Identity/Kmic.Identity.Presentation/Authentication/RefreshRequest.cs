namespace Kmic.Identity.Presentation.Authentication;

/// <summary>
/// 刷新请求：移动端在请求体里给出刷新令牌，网页客户端用 Cookie，不需要请求体
/// </summary>
/// <param name="RefreshToken">刷新令牌</param>
public sealed record RefreshRequest(string? RefreshToken);

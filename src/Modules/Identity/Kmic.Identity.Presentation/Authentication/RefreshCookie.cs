namespace Kmic.Identity.Presentation.Authentication;

/// <summary>
/// 网页客户端的刷新令牌 Cookie
/// </summary>
/// <remarks>
/// __Host- 前缀要求 Secure、Path=/ 且不设置 Domain；HttpOnly 禁止脚本读取，SameSite=Strict 限制跨站携带 Cookie
/// </remarks>
internal static class RefreshCookie
{
    public const string Name = "__Host-kmic-rt";

    public static void Set(HttpResponse response, string token, DateTimeOffset expires) =>
        response.Cookies.Append(Name, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = expires,
        });

    public static void Clear(HttpResponse response) =>
        response.Cookies.Delete(Name, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Path = "/" });
}

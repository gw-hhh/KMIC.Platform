namespace Kmic.Identity.Presentation.Authentication;

// __Host- 前缀要求启用 Secure、Path=/ 且不设置 Domain
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

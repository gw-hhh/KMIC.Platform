using System.Security.Claims;

namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>
/// 限流时用来区分请求方的键：登录了按用户，没有登录按 IP
/// </summary>
internal static class ClientKey
{
    public static bool IsUser(HttpContext http) => UserId(http) is not null;

    public static string Of(HttpContext http) =>
        UserId(http) is { } user ? $"user:{user}" : $"ip:{http.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

    // JWT 的 sub，经过默认的声明映射后也可能是 NameIdentifier
    private static string? UserId(HttpContext http) =>
        http.User.Identity is { IsAuthenticated: true }
            ? http.User.FindFirstValue("sub") ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
}

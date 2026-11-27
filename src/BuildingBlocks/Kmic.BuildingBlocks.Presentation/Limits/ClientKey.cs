using System.Security.Claims;

namespace Kmic.BuildingBlocks.Presentation.Limits;

// 限流分区键：已认证请求按用户 ID，匿名请求按客户端 IP
internal static class ClientKey
{
    public static bool IsUser(HttpContext http) => UserId(http) is not null;

    public static string Of(HttpContext http) =>
        UserId(http) is { } user ? $"user:{user}" : $"ip:{http.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

    // JWT 的 sub 声明经默认声明映射后可能转换为 NameIdentifier
    private static string? UserId(HttpContext http) =>
        http.User.Identity is { IsAuthenticated: true }
            ? http.User.FindFirstValue("sub") ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
}

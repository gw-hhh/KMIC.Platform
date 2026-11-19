using System.Net;
using System.Net.Sockets;

namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 判断请求是不是直接来自内网
/// </summary>
/// <remarks>
/// 经反向代理转发的请求一律不算内网
/// </remarks>
internal static class InternalRequests
{
    private static readonly string[] _forwardingHeaders = ["X-Forwarded-For", "Forwarded", "X-Real-IP"];

    public static bool IsInternal(HttpContext http)
    {
        ArgumentNullException.ThrowIfNull(http);

        if (_forwardingHeaders.Any(http.Request.Headers.ContainsKey))
        {
            return false;
        }

        // 没有远端地址表示不是 IP 连接（本机套接字、进程内的测试服务器）
        return http.Connection.RemoteIpAddress is not { } address || IsPrivate(address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address);
    }

    private static bool IsPrivate(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        var bytes = address.GetAddressBytes();
        return address.AddressFamily switch
        {
            AddressFamily.InterNetwork => bytes is [10, ..] or [172, >= 16 and <= 31, ..] or [192, 168, ..] or [169, 254, ..],
            AddressFamily.InterNetworkV6 => address.IsIPv6LinkLocal || (bytes[0] & 0xFE) == 0xFC,
            _ => false,
        };
    }
}

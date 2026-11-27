using System.Net;
using System.Net.Sockets;

namespace Kmic.BuildingBlocks.Hosting.Health;

// 判断是否为内网直连请求（经反向代理转发均不视为直连）
internal static class InternalRequests
{
    // 转发头中间件处理可信代理请求后会清除 X-Forwarded-For 并写入 X-Original-For，需一并识别
    private static readonly string[] _forwardingHeaders = ["X-Forwarded-For", "Forwarded", "X-Real-IP", "X-Original-For"];

    public static bool IsInternal(HttpContext http)
    {
        ArgumentNullException.ThrowIfNull(http);

        if (_forwardingHeaders.Any(http.Request.Headers.ContainsKey))
        {
            return false;
        }

        // 无远端地址表明非 IP 连接（如本机套接字或进程内测试服务器）
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

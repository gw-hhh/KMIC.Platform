using System.Net;
using System.Net.Sockets;

namespace Kmic.BuildingBlocks.Testing;

/// <summary>
/// 为测试容器挑映射到本机的端口
/// </summary>
/// <remarks>
/// Windows 默认把 49152 到 65535 当作临时端口，本机每次发起连接都从里面随机取源端口。容器映射的端口如果也落在这个范围里，
/// 源端口偶尔会和目标端口相同，连接就失败，报“请求的地址无效”，表现为整套测试里随机有一两个失败。所以映射端口选在这个范围之外
/// </remarks>
internal static class TestPorts
{
    private const int First = 20000;
    private const int Last = 40000;

    /// <summary>
    /// 随机挑一个当前没人占用的端口
    /// </summary>
    /// <exception cref="InvalidOperationException">试了很多次都没有可用的端口</exception>
    public static int Free()
    {
        for (var attempt = 0; attempt < 200; attempt++)
        {
            var port = Random.Shared.Next(First, Last);
            try
            {
                using var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                return port;
            }
            catch (SocketException)
            {
                // 被占用或系统保留了这个端口，换一个
            }
        }

        throw new InvalidOperationException($"在 {First} 到 {Last} 之间找不到可用的端口。");
    }
}

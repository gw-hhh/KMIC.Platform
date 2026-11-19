using System.Net;
using System.Net.Sockets;

namespace Kmic.BuildingBlocks.Testing;

/// <summary>
/// 为测试容器挑本机端口，避开 Windows 的临时端口段
/// </summary>
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

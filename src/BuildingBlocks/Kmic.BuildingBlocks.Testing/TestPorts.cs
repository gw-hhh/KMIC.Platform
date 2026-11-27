using System.Net;
using System.Net.Sockets;

namespace Kmic.BuildingBlocks.Testing;

// 分配本机可用端口，避开 Windows 动态临时端口段
internal static class TestPorts
{
    private const int First = 20000;
    private const int Last = 40000;

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
                // 该端口已被占用或受系统保留，继续尝试下一个
            }
        }

        throw new InvalidOperationException($"在 {First} 至 {Last} 范围内未找到可用端口。");
    }
}

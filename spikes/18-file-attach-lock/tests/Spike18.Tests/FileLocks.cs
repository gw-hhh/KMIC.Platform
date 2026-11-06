using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Spike18.Tests;

internal static class FileLocks
{
    // 05-11 第 3.3 节补充说明：SHA-256("Kmic.Files:" + 文件 ID) 的前 8 字节
    public static long Key(Guid fileId) =>
        BinaryPrimitives.ReadInt64BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes($"Kmic.Files:{fileId:D}")));
}

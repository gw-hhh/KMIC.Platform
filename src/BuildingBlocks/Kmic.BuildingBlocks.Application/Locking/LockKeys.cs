using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Kmic.BuildingBlocks.Application.Locking;

/// <summary>
/// 咨询锁的键：由范围名和对象 ID 派生
/// </summary>
public static class LockKeys
{
    /// <summary>
    /// <c>SHA-256("Kmic." + 范围 + ":" + ID)</c> 的前 8 个字节，按大端读成有符号 64 位整数
    /// </summary>
    public static long Of(string scope, Guid id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"Kmic.{scope}:{id:D}"));
        return BinaryPrimitives.ReadInt64BigEndian(hash);
    }
}

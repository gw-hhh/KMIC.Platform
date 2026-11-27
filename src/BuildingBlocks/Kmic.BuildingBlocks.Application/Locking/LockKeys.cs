using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Kmic.BuildingBlocks.Application.Locking;

/// <summary>由范围名与对象 ID 派生的咨询锁键。</summary>
public static class LockKeys
{
    /// <summary>取 <c>SHA-256("Kmic." + 范围 + ":" + ID)</c> 的前 8 字节，按大端序转为有符号 64 位整数。</summary>
    public static long Of(string scope, Guid id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"Kmic.{scope}:{id:D}"));
        return BinaryPrimitives.ReadInt64BigEndian(hash);
    }
}

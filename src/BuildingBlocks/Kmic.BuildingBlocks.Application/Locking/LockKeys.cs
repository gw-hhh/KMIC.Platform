using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Kmic.BuildingBlocks.Application.Locking;

/// <summary>
/// 咨询锁的键：由范围名和对象 ID 派生的 64 位值
/// </summary>
/// <remarks>
/// 互斥的双方（比如业务命令取共享锁、文件清理取排他锁）要用同样的规则算出同一个键，所以规则放在这里
/// </remarks>
public static class LockKeys
{
    /// <summary>
    /// <c>SHA-256("Kmic." + 范围 + ":" + ID)</c> 的前 8 个字节，按大端读成有符号 64 位整数
    /// </summary>
    /// <param name="scope">范围名，如 Files</param>
    /// <param name="id">对象 ID</param>
    public static long Of(string scope, Guid id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"Kmic.{scope}:{id:D}"));
        return BinaryPrimitives.ReadInt64BigEndian(hash);
    }
}

using System.Globalization;

namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// 存储键的写法
/// </summary>
public static class FileStorageKeys
{
    /// <summary>
    /// 按环境、年月和文件 ID 生成存储键：{环境}/{yyyy}/{MM}/{fileId}
    /// </summary>
    /// <remarks>
    /// 不含原始文件名：防路径穿越、防覆盖、防通过文件名猜测
    /// </remarks>
    /// <param name="environment">环境名，如 prod、test</param>
    /// <param name="fileId">文件 ID</param>
    /// <param name="at">上传时间</param>
    public static string For(string environment, Guid fileId, DateTimeOffset at)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(environment);
        if (environment.Contains('/', StringComparison.Ordinal) || environment.Contains('.', StringComparison.Ordinal))
        {
            throw new ArgumentException($"环境名不能含“/”或“.”：“{environment}”。", nameof(environment));
        }

        var utc = at.ToUniversalTime();
        return string.Create(CultureInfo.InvariantCulture, $"{environment}/{utc:yyyy}/{utc:MM}/{fileId:N}");
    }

    /// <summary>
    /// 从存储键取出文件 ID，不是这个写法的键（别的程序放进来的对象）返回 false
    /// </summary>
    /// <param name="key">存储键</param>
    /// <param name="environment">环境名，键必须以它开头</param>
    /// <param name="fileId">文件 ID</param>
    public static bool TryGetFileId(string key, string environment, out Guid fileId)
    {
        fileId = Guid.Empty;
        var parts = key.Split('/');
        return parts is [var env, { Length: 4 } year, { Length: 2 } month, var id]
            && string.Equals(env, environment, StringComparison.Ordinal)
            && year.All(char.IsAsciiDigit)
            && month.All(char.IsAsciiDigit)
            && Guid.TryParseExact(id, "N", out fileId);
    }

    /// <summary>
    /// 文件 ID 里的创建时间：文件 ID 是 UUID v7，前 48 位是毫秒时间戳。不是 v7 的 ID 返回空
    /// </summary>
    /// <param name="fileId">文件 ID</param>
    public static DateTimeOffset? CreatedAt(Guid fileId)
    {
        if (fileId.Version != 7)
        {
            return null;
        }

        Span<byte> bytes = stackalloc byte[16];
        fileId.TryWriteBytes(bytes, bigEndian: true, out _);
        var milliseconds = ((long)bytes[0] << 40) | ((long)bytes[1] << 32) | ((long)bytes[2] << 24) | ((long)bytes[3] << 16) | ((long)bytes[4] << 8) | bytes[5];
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
    }
}

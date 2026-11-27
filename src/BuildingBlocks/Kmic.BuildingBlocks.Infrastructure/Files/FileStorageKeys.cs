using System.Globalization;

namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>文件存储键构造与解析</summary>
public static class FileStorageKeys
{
    /// <summary>生成存储键：{环境}/{yyyy}/{MM}/{fileId}</summary>
    public static string For(string environment, Guid fileId, DateTimeOffset at)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(environment);
        if (environment.Contains('/', StringComparison.Ordinal) || environment.Contains('.', StringComparison.Ordinal))
        {
            throw new ArgumentException($"环境名称不能包含“/”或“.”：“{environment}”。", nameof(environment));
        }

        var utc = at.ToUniversalTime();
        return string.Create(CultureInfo.InvariantCulture, $"{environment}/{utc:yyyy}/{utc:MM}/{fileId:N}");
    }

    /// <summary>从存储键解析文件 ID</summary>
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

    /// <summary>从 UUID v7 文件 ID 提取创建时间</summary>
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

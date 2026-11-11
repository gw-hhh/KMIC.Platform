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
}

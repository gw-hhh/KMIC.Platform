namespace Kmic.Files.Application;

/// <summary>
/// 对账用的存储访问：列出本环境的对象，核对和删除单个对象
/// </summary>
/// <remarks>
/// 仅列出当前环境前缀下、键符合 Files 格式的对象
/// </remarks>
public interface IStorageInventory
{
    /// <summary>
    /// 列出本环境下可识别的文件对象
    /// </summary>
    IAsyncEnumerable<InventoryObject> ListAsync(CancellationToken ct);

    /// <summary>
    /// 检查对象是否存在
    /// </summary>
    /// <remarks>
    /// S3 指定版本号时检查该版本，否则检查当前对象；本地存储忽略版本号
    /// </remarks>
    Task<bool> ExistsAsync(string key, string? versionId, CancellationToken ct);

    /// <summary>
    /// 删除对象，不存在时不报错
    /// </summary>
    Task DeleteAsync(string key, CancellationToken ct);
}

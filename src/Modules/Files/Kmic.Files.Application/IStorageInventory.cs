namespace Kmic.Files.Application;

/// <summary>对账用存储访问接口：列举本环境对象，并核对或删除单个对象</summary>
public interface IStorageInventory
{
    /// <summary>列出本环境下可识别的文件对象</summary>
    IAsyncEnumerable<InventoryObject> ListAsync(CancellationToken ct);

    /// <summary>检查对象是否存在（本地存储忽略版本号）</summary>
    Task<bool> ExistsAsync(string key, string? versionId, CancellationToken ct);

    /// <summary>删除对象，不存在时不报错</summary>
    Task DeleteAsync(string key, CancellationToken ct);
}

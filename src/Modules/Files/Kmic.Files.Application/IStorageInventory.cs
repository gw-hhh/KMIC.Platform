namespace Kmic.Files.Application;

/// <summary>
/// 对账用的存储访问：列出本环境的对象，核对和删除单个对象
/// </summary>
/// <remarks>
/// 只列出键符合 Files 写法的对象，别的程序放进同一个桶里的对象不在其中
/// </remarks>
public interface IStorageInventory
{
    /// <summary>
    /// 列出本环境下的全部对象
    /// </summary>
    IAsyncEnumerable<InventoryObject> ListAsync(CancellationToken ct);

    /// <summary>
    /// 对象是否还在：给了版本号就看那个版本
    /// </summary>
    /// <param name="key">存储键</param>
    /// <param name="versionId">对象版本号</param>
    /// <param name="ct">取消令牌</param>
    Task<bool> ExistsAsync(string key, string? versionId, CancellationToken ct);

    /// <summary>
    /// 删除对象，不存在时不报错
    /// </summary>
    /// <param name="key">存储键</param>
    /// <param name="ct">取消令牌</param>
    Task DeleteAsync(string key, CancellationToken ct);
}

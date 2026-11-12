namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 权限点与范围对象目录在数据库里的存储：模块内的框架表，只由同步与清理写
/// </summary>
/// <remarks>
/// 和命令同一个事务。这些表不是聚合：没有审计列和版本号，一次同步整体写入
/// </remarks>
public interface IPermissionCatalogStore
{
    /// <summary>
    /// 同步之前先拿到这把锁，直到事务结束：多个实例同时启动时只有一个真正写入，后到的看到的已经是同步好的结果
    /// </summary>
    Task LockSyncAsync(CancellationToken ct);

    /// <summary>
    /// 上次同步的目录哈希；还没有同步过时为空
    /// </summary>
    Task<string?> GetHashAsync(CancellationToken ct);

    /// <summary>
    /// 记下这次同步的目录哈希和时间
    /// </summary>
    Task SaveHashAsync(string hash, DateTimeOffset now, CancellationToken ct);

    /// <summary>
    /// 让数据库里的目录与代码一致：按声明改名、新增、更新，代码里没有的标为退役，重新出现的恢复
    /// </summary>
    Task<CatalogChanges> ApplyAsync(CatalogContent content, DateTimeOffset now, CancellationToken ct);

    /// <summary>
    /// 退役时间早于给定时间的权限点编码
    /// </summary>
    Task<IReadOnlyList<string>> RetiredPermissionCodesAsync(DateTimeOffset retiredBefore, CancellationToken ct);

    /// <summary>
    /// 退役时间早于给定时间的范围对象编码
    /// </summary>
    Task<IReadOnlyList<string>> RetiredScopeObjectCodesAsync(DateTimeOffset retiredBefore, CancellationToken ct);

    /// <summary>
    /// 删除这些已退役的权限点与范围对象
    /// </summary>
    Task DeleteRetiredAsync(IReadOnlyCollection<string> permissionCodes, IReadOnlyCollection<string> scopeObjectCodes, CancellationToken ct);
}

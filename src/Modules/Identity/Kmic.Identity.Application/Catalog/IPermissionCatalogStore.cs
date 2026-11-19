namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 权限点与范围对象的目录存储，仅供同步与清理更新
/// </summary>
public interface IPermissionCatalogStore
{
    /// <summary>
    /// 同步前取事务级咨询锁，让多个实例的同步依次执行，后到的读到前一个提交的结果
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

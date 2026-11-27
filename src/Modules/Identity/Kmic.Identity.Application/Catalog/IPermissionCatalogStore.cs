namespace Kmic.Identity.Application.Catalog;

/// <summary>权限点与范围对象的目录存储，仅供同步与清理更新</summary>
public interface IPermissionCatalogStore
{
    /// <summary>同步前获取事务级咨询锁，确保多实例串行同步并读取前序提交结果</summary>
    Task LockSyncAsync(CancellationToken ct);

    /// <summary>获取上次同步的目录哈希；首次同步时返回 null</summary>
    Task<string?> GetHashAsync(CancellationToken ct);

    /// <summary>记录本次同步的目录哈希与同步时间</summary>
    Task SaveHashAsync(string hash, DateTimeOffset now, CancellationToken ct);

    /// <summary>将代码声明的权限目录同步至数据库</summary>
    Task<CatalogChanges> ApplyAsync(CatalogContent content, DateTimeOffset now, CancellationToken ct);

    /// <summary>查询已废弃且早于指定时间的权限点编码</summary>
    Task<IReadOnlyList<string>> RetiredPermissionCodesAsync(DateTimeOffset retiredBefore, CancellationToken ct);

    /// <summary>查询已废弃且早于指定时间的范围对象编码</summary>
    Task<IReadOnlyList<string>> RetiredScopeObjectCodesAsync(DateTimeOffset retiredBefore, CancellationToken ct);

    /// <summary>删除指定的已废弃权限点与范围对象</summary>
    Task DeleteRetiredAsync(IReadOnlyCollection<string> permissionCodes, IReadOnlyCollection<string> scopeObjectCodes, CancellationToken ct);
}

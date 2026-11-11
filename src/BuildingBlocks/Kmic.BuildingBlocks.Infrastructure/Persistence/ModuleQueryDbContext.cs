using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模块查询上下文基类
/// </summary>
/// <remarks>
/// 默认不跟踪，不能保存。用读池的连接，查不到当前命令还没提交的修改，所以命令处理器不能注入它
/// </remarks>
public abstract class ModuleQueryDbContext : ModuleDbContext
{
    /// <summary>
    /// 供模块查询上下文调用
    /// </summary>
    protected ModuleQueryDbContext(DbContextOptions options, ModuleDbContextServices services)
        : base(options, services) =>
        base.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

    /// <summary>
    /// 查询上下文不能保存
    /// </summary>
    /// <exception cref="InvalidOperationException">总是抛出</exception>
    public sealed override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("查询上下文不能保存。");
}

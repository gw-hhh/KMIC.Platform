using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模块查询上下文基类：默认不跟踪，不能保存
/// </summary>
/// <remarks>
/// 使用读池的连接，看不到当前命令尚未提交的修改；命令处理器不得注入它（架构测试）
/// </remarks>
public abstract class ModuleQueryDbContext : ModuleDbContext
{
    /// <summary>
    /// 由模块查询上下文调用
    /// </summary>
    protected ModuleQueryDbContext(DbContextOptions options, ModuleDbContextServices services)
        : base(options, services) =>
        base.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

    /// <summary>
    /// 查询上下文不能保存
    /// </summary>
    /// <exception cref="InvalidOperationException">总是抛出</exception>
    public sealed override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("查询上下文不能保存：修改数据要经命令与仓储。");
}

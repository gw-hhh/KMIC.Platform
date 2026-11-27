using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>模块只读查询上下文基类（禁用变更跟踪与保存，不可见未提交更改，命令处理器禁止注入）</summary>
public abstract class ModuleQueryDbContext : ModuleDbContext
{
    protected ModuleQueryDbContext(DbContextOptions options, ModuleDbContextServices services)
        : base(options, services) =>
        base.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

    /// <summary>禁用保存操作</summary>
    public sealed override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("查询上下文不能保存。");
}

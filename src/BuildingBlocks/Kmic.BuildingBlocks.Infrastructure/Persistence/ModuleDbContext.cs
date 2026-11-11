using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Kmic.BuildingBlocks.Infrastructure.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模块 DbContext 基类
/// </summary>
/// <remarks>
/// 写入上下文直接继承它，查询上下文继承 ModuleQueryDbContext，两者共用同一个模型。
/// 只有命令事务能保存，处理器自己调用 SaveChangesAsync 会抛异常
/// </remarks>
public abstract class ModuleDbContext : DbContext
{
    /// <summary>
    /// 供模块上下文调用
    /// </summary>
    protected ModuleDbContext(DbContextOptions options, ModuleDbContextServices services)
        : base(options)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));

        // 级联删除推迟到保存时：软删除先把聚合根的删除改成修改，子实体就不会被级联删掉
        base.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
    }

    /// <summary>
    /// 模块名
    /// </summary>
    public abstract string Module { get; }

    // 数据权限过滤器通过这两个成员读取当前请求的范围
    internal bool DataScopeUnrestricted => Services.DataScope.Unrestricted;

    // 仓储基类通过它取当前身份、时间和安全事件
    internal ModuleDbContextServices Services { get; }

    /// <summary>
    /// 智能枚举和单值值对象所在的程序集：上下文所在程序集，以及它引用的 Kmic.* 程序集
    /// </summary>
    protected virtual IEnumerable<Assembly> ModelAssemblies =>
        GetType().Assembly.GetReferencedAssemblies()
            .Where(name => name.Name?.StartsWith("Kmic.", StringComparison.Ordinal) == true)
            .Select(Assembly.Load)
            .Prepend(GetType().Assembly);

    /// <summary>
    /// 不支持同步保存
    /// </summary>
    /// <exception cref="NotSupportedException">总是抛出</exception>
    public sealed override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new NotSupportedException("不支持同步保存，请用 SaveChangesAsync。");

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        Services.SaveGate.EnsureOpen();

        // 先检测变更：离开集合的子实体要到这一步才变成 Deleted
        ChangeTracker.DetectChanges();
        var now = Services.Time.GetUtcNow();
        SaveRules.Apply(this, Services.Execution, now);
        StateHistory.Write(this, Services.Execution, now);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    internal EffectiveScope DataScopeFor(string scopeObject) => Services.DataScope.For(scopeObject);

    /// <inheritdoc />
    protected sealed override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        // 迁移历史放在本模块的 schema 里，各模块互不影响
        optionsBuilder
            .UseNpgsql(npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Module))
            .ReplaceService<IMigrationsSqlGenerator, AppendOnlyMigrationsSqlGenerator>();
    }

    /// <inheritdoc />
    protected sealed override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        // 表名取实体类名，不取 DbSet 属性名
        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();

        // 领域事件和状态流转只在内存里传递，不忽略的话，EF 会顺着聚合根的属性把它们当成实体
        configurationBuilder.IgnoreAny<IDomainEvent>();
        configurationBuilder.IgnoreAny<StateTransition>();
        configurationBuilder.RegisterAllInFrameworkIdConverters();
        ValueTypeConventions.Register(configurationBuilder, ModelAssemblies);
        configurationBuilder.Conventions.Add(_ => new ModelValidationConvention(Module, Services.ScopeObjects));
        ConfigureModuleConventions(configurationBuilder);
    }

    /// <inheritdoc />
    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Module);
        ConfigureModel(modelBuilder);
        FrameworkTables.Apply(modelBuilder);
        StateHistory.Map(modelBuilder);
        ModelConventions.Apply(modelBuilder);
        DataScopeFilters.Apply(modelBuilder, this);
    }

    /// <summary>
    /// 注册本模块强类型 ID 的转换器
    /// </summary>
    /// <remarks>
    /// 调用 Vogen 按 [EfCoreConverter&lt;T&gt;] 生成的 RegisterAllIn{标记类}()
    /// </remarks>
    protected abstract void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder);

    /// <summary>
    /// 应用实体配置，默认是上下文所在程序集里的全部配置
    /// </summary>
    protected virtual void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
}

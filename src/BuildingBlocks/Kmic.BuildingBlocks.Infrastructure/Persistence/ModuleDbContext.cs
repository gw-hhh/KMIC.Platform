using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模块 DbContext 基类：写入上下文直接继承，查询上下文继承 ModuleQueryDbContext；两者共用同一套模型
/// </summary>
/// <remarks>
/// 模型的构建顺序固定：默认 schema → 模块的实体配置 → 框架约定（审计、软删除、并发版本、值对象列名、CHECK）→ 数据范围过滤器 → 定稿时模型校验。
/// 保存只能由事务行为发起，并依次执行软删除、审计字段、聚合版本三步
/// </remarks>
public abstract class ModuleDbContext : DbContext
{
    /// <summary>
    /// 由模块上下文调用
    /// </summary>
    protected ModuleDbContext(DbContextOptions options, ModuleDbContextServices services)
        : base(options)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));

        // 级联删除推迟到保存时：软删除先把聚合根的删除改成修改，子实体就不会被级联删掉
        base.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
    }

    /// <summary>
    /// 模块名，也是 schema 名
    /// </summary>
    public abstract string Module { get; }

    // 数据范围过滤器经这两个成员读取当前请求的范围
    internal bool DataScopeUnrestricted => Services.DataScope.Unrestricted;

    // 仓储基类经它取得执行身份、时间与安全事件
    internal ModuleDbContextServices Services { get; }

    /// <summary>
    /// 从中查找智能枚举与单值值对象的程序集：上下文所在程序集及其引用的 Kmic.* 程序集（模块的 Domain 在其中）
    /// </summary>
    protected virtual IEnumerable<Assembly> ModelAssemblies =>
        GetType().Assembly.GetReferencedAssemblies()
            .Where(name => name.Name?.StartsWith("Kmic.", StringComparison.Ordinal) == true)
            .Select(Assembly.Load)
            .Prepend(GetType().Assembly);

    /// <summary>
    /// 只支持异步保存
    /// </summary>
    /// <exception cref="NotSupportedException">总是抛出</exception>
    public sealed override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new NotSupportedException("只支持 SaveChangesAsync，由事务行为调用。");

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        Services.SaveGate.EnsureOpen();

        // 先检测变更：从聚合集合中移除的子实体要在这一步才变成 Deleted
        ChangeTracker.DetectChanges();
        SaveRules.Apply(this, Services.Execution, Services.Time.GetUtcNow());
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    internal EffectiveScope DataScopeFor(string scopeObject) => Services.DataScope.For(scopeObject);

    /// <inheritdoc />
    protected sealed override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        // 表名取实体类名，不取 DbSet 属性名
        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();

        // 领域事件与状态流转只在内存中传递；不忽略类型的话 EF 会经聚合根的属性把它们发现为实体
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
        ModelConventions.Apply(modelBuilder);
        DataScopeFilters.Apply(modelBuilder, this);
    }

    /// <summary>
    /// 注册本模块强类型 ID 的转换器：调用 Vogen 按 [EfCoreConverter&lt;T&gt;] 标记生成的 RegisterAllIn{标记类}()
    /// </summary>
    protected abstract void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder);

    /// <summary>
    /// 应用本模块的实体配置，默认是上下文所在程序集中的全部 IEntityTypeConfiguration
    /// </summary>
    protected virtual void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
}

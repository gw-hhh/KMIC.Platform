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

/// <summary>模块写入上下文基类（仅允许在命令事务内保存，处理器直接调用 SaveChangesAsync 将抛异常）</summary>
public abstract class ModuleDbContext : DbContext
{
    protected ModuleDbContext(DbContextOptions options, ModuleDbContextServices services)
        : base(options)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));

        // 将级联删除时机推迟至保存阶段：软删除在保存前将聚合根状态由删除转为修改，从而避免子实体被连带级联删除
        base.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
    }

    public abstract string Module { get; }

    // 数据权限过滤器通过这两个成员读取当前请求的数据范围
    internal bool DataScopeUnrestricted => Services.DataScope.Unrestricted;

    // 仓储基类通过它获取当前身份上下文、系统时钟与安全事件写入器
    internal ModuleDbContextServices Services { get; }

    /// <summary>扫描智能枚举与单值值对象的程序集范围（含当前上下文及引用的全部 Kmic.* 程序集）</summary>
    protected virtual IEnumerable<Assembly> ModelAssemblies =>
        GetType().Assembly.GetReferencedAssemblies()
            .Where(name => name.Name?.StartsWith("Kmic.", StringComparison.Ordinal) == true)
            .Select(Assembly.Load)
            .Prepend(GetType().Assembly);

    /// <summary>禁用同步保存，请使用 SaveChangesAsync</summary>
    public sealed override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new NotSupportedException("不支持同步保存，请用 SaveChangesAsync。");

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        Services.SaveGate.EnsureOpen();

        // 先触发变更检测，确保从集合中移除的子实体被标记为 Deleted 后再执行保存规则
        ChangeTracker.DetectChanges();
        var now = Services.Time.GetUtcNow();
        SaveRules.Apply(this, Services.Execution, now);
        StateHistory.Write(this, Services.Execution, now);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>在命令管道外保存更改（仅供迁移及框架基础设施调用，业务代码严禁使用）</summary>
    public async Task<int> SaveForToolAsync(CancellationToken ct)
    {
        using (Services.SaveGate.Open())
        {
            return await SaveChangesAsync(ct);
        }
    }

    internal EffectiveScope DataScopeFor(string scopeObject) => Services.DataScope.For(scopeObject);

    protected sealed override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        // 迁移历史表存放于各自模块的 schema 下，实现模块间迁移记录隔离
        optionsBuilder
            .UseNpgsql(npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Module))
            .ReplaceService<IMigrationsSqlGenerator, AppendOnlyMigrationsSqlGenerator>();
    }

    protected sealed override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        // 约定表名与实体类名保持一致，而非使用 DbSet 属性名
        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();

        // 领域事件与状态流转仅用于内存传递；若不显式忽略，EF Core 会沿聚合根属性将其误识别为实体类型
        configurationBuilder.IgnoreAny<IDomainEvent>();
        configurationBuilder.IgnoreAny<StateTransition>();
        configurationBuilder.RegisterAllInFrameworkIdConverters();
        ValueTypeConventions.Register(configurationBuilder, ModelAssemblies);
        configurationBuilder.Conventions.Add(_ => new ModelValidationConvention(Module, Services.ScopeObjects));
        ConfigureModuleConventions(configurationBuilder);
    }

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

    /// <summary>注册本模块强类型 ID 转换器（调用 Vogen 生成的 RegisterAllIn* 方法）</summary>
    protected abstract void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder);

    /// <summary>配置实体映射，默认扫描当前程序集内的全部配置类</summary>
    protected virtual void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
}

using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Exports;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Caching;
using Kmic.BuildingBlocks.Infrastructure.History;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Observability;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    /// <summary>注册数据访问、事务管道、消息队列与后台作业等持久化基础设施服务</summary>
    public static IServiceCollection AddKmicPersistence(this IServiceCollection services, Action<DatabaseOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.AddOptions<IdempotencyOptions>();
        services.TryAddSingleton<DataSources>();
        services.TryAddScoped<SaveChangesGate>();
        services.TryAddScoped<ModuleDbContextServices>();
        services.AddKmicCache();
        services.AddKmicSlowQueryLog();
        services.AddModuleContextRegistries();
        services.AddOperationsAdminServices();
        services.TryAddScoped<CommandTransaction>();
        services.TryAddScoped<ITransactionLocks, TransactionLocks>();
        services.TryAddScoped<IUnfinishedMessageProbe, UnfinishedMessageProbe>();
        services.TryAddScoped<OutboxWriter>();
        services.TryAddSingleton<MessagingAdmission>();
        services.TryAddSingleton<ModuleFrameworkTables>();
        services.TryAddSingleton<OutboxDispatcher>();
        services.TryAddScoped<CurrentInboxMessage>();
        services.TryAddSingleton<InboxProcessor>();
        services.TryAddScoped(typeof(IReadModelWriter<>), typeof(ReadModelWriter<>));
        services.TryAddScoped(typeof(IReadModelMaintenance<>), typeof(ReadModelWriter<>));
        services.TryAddScoped<IBackgroundJobs, BackgroundJobs>();
        services.TryAddSingleton<JobStore>();
        services.TryAddSingleton<RebuildRunStore>();
        services.TryAddScoped<INumberGenerator, NumberGenerator>();
        services.TryAddScoped<AuditRecorder>();
        services.TryAddScoped<ReadAuditWriter>();
        services.TryAddScoped<IAuditContext>(provider => provider.GetRequiredService<AuditRecorder>());
        services.AddOptions<AuditOptions>();
        services.TryAddSingleton<JobExecutor>();
        services.TryAddSingleton<IQueryScopeFactory, QueryScopeFactory>();
        services.TryAddScoped<IIndependentDatabase, IndependentDatabase>();
        services.AddOptions<JobOptions>();
        services.AddOptions<MessagingOptions>();
        services.TryAddScoped<IAfterCommit>(provider => provider.GetRequiredService<CommandTransaction>());
        services.TryAddScoped<DomainEventDispatcher>();
        services.TryAddScoped<QueryVisibilityGuard>();
        services.TryAddScoped<IDataScopeSql, DataScopeSql>();
        return services;
    }

    /// <summary>注册模块的写入与只读查询数据库上下文</summary>
    public static IServiceCollection AddModuleDbContexts<TContext, TQueryContext, TQueryDb>(this IServiceCollection services)
        where TContext : ModuleDbContext
        where TQueryContext : ModuleQueryDbContext, TQueryDb
        where TQueryDb : class
    {
        ArgumentNullException.ThrowIfNull(services);
        if (typeof(TContext).IsAssignableTo(typeof(ModuleQueryDbContext)))
        {
            throw new InvalidOperationException($"{typeof(TContext).Name} 是查询上下文，写入上下文要直接继承 ModuleDbContext。");
        }

        // 禁用 DbContext 池化：查询过滤器依赖当前用户及数据范围，若池化复用未彻底重置，可能导致跨请求权限上下文泄漏
        // 首次获取数据库上下文时，依据执行入口来源选择对应的连接池
        services.AddDbContext<TContext>((provider, options) =>
            AddFrameworkExtensions(
                provider, options.UseNpgsql(provider.GetRequiredService<DataSources>().Transactions(EntryOrigin(provider)))));
        services.AddDbContext<TQueryContext>((provider, options) =>
        {
            AddFrameworkExtensions(
                provider,
                options.UseNpgsql(
                provider.GetRequiredService<DataSources>().Reads(EntryOrigin(provider)),
                npgsql => npgsql.ExecutionStrategy(dependencies => new QueryRetryStrategy(dependencies))));
            if (provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.CheckQueryVisibility)
            {
                options.AddInterceptors(provider.GetRequiredService<QueryVisibilityGuard>());
            }
        });
        services.AddScoped<TQueryDb>(provider => provider.GetRequiredService<TQueryContext>());
        services.AddScoped<IExportReader<TQueryDb>, ExportReader<TQueryDb, TQueryContext>>();
        services.AddSingleton(new ModuleWriteContext(ModuleNames.Of(typeof(TContext)), typeof(TContext)));
        services.AddSingleton(new ModuleQueryContext(ModuleNames.Of(typeof(TContext)), typeof(TQueryContext)));
        return services;
    }

    /// <summary>注册仅按 ID 读写即可满足需求的聚合通用仓储；需按条件查找或更新时请编写专用仓储</summary>
    /// <param name="notFound">找不到时返回的错误，如 <c>SampleErrors.CategoryNotFound</c></param>
    /// <param name="aggregate">读取时包含的子实体查询，为空表示只读取聚合根</param>
    public static IServiceCollection AddRepository<TContext, TAggregate, TId>(
        this IServiceCollection services,
        Error notFound,
        Func<IQueryable<TAggregate>, IQueryable<TAggregate>>? aggregate = null)
        where TContext : ModuleDbContext
        where TAggregate : AggregateRoot<TId>
        where TId : struct, IStronglyTypedId
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(notFound);
        if (services.Any(d => d.ServiceType == typeof(IRepository<TAggregate, TId>)))
        {
            throw new InvalidOperationException($"{typeof(TAggregate).Name} 的仓储已经注册过了。");
        }

        services.AddScoped<IRepository<TAggregate, TId>>(
            provider => new ModuleRepository<TContext, TAggregate, TId>(provider.GetRequiredService<TContext>(), notFound, aggregate));
        return services;
    }

    internal static void AddFrameworkExtensions(IServiceProvider provider, DbContextOptionsBuilder options)
    {
        ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(new KmicFunctionsExtension());
        options.AddInterceptors(provider.GetRequiredService<SlowQueryInterceptor>());
    }

    // 供 Operations 模块管理视图使用：作业、消息与路由的汇总统计及运维操作均由框架层统一读写各模块框架表
    private static void AddOperationsAdminServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IModuleJobStores, ModuleJobStores>();
        services.TryAddSingleton<IModuleMessageStores, ModuleMessageStores>();
        services.TryAddSingleton<IAbandonGuard, AbandonGuard>();
        services.TryAddSingleton<IMessageRouteAdmin, MessageRouteAdmin>();
        services.TryAddSingleton<IReadModelRebuildAdmin, RebuildAdmin>();
    }

    // 注册模块上下文及依赖它们的业务历史查询；历史查询通过工厂注册，使不含 Identity 的宿主（如迁移程序）在构建容器时无需校验 IActorReferences
    private static void AddModuleContextRegistries(this IServiceCollection services)
    {
        services.TryAddScoped<ModuleWriteContexts>();
        services.TryAddScoped<ModuleQueryContexts>();
        services.TryAddScoped<IStateHistoryQuery>(provider => new StateHistoryQuery(
            provider.GetRequiredService<ModuleQueryContexts>(), provider.GetRequiredService<IActorReferences>()));
    }

    private static CallOrigin EntryOrigin(IServiceProvider provider) => provider.GetRequiredService<ExecutionScope>().EntryOrigin;
}

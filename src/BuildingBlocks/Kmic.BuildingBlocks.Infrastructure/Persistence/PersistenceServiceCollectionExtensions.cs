using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Exports;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Caching;
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

/// <summary>
/// 数据访问服务注册
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// 注册数据访问服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置连接串和连接池</param>
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
        services.TryAddScoped<ModuleWriteContexts>();
        services.TryAddScoped<CommandTransaction>();
        services.TryAddScoped<OutboxWriter>();
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
        return services;
    }

    /// <summary>
    /// 注册模块的数据库上下文
    /// </summary>
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

        // 不用 DbContext 池：过滤器要读当前用户和他的数据权限，池里的实例没重置干净，就会拿上一个请求的权限去查。
        // 第一次用到上下文时，按入口的来源选连接池
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
        return services;
    }

    internal static void AddFrameworkExtensions(IServiceProvider provider, DbContextOptionsBuilder options)
    {
        ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(new KmicFunctionsExtension());
        options.AddInterceptors(provider.GetRequiredService<SlowQueryInterceptor>());
    }

    private static CallOrigin EntryOrigin(IServiceProvider provider) => provider.GetRequiredService<ExecutionScope>().EntryOrigin;
}

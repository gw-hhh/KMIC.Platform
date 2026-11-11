using Kmic.BuildingBlocks.Application.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 注册数据访问
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// 注册连接池与模块上下文共用的框架服务；宿主调用一次
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">填写连接串与连接池上限，宿主从 ConnectionStrings:Kmic 与 Kmic:Database 读取</param>
    public static IServiceCollection AddKmicPersistence(this IServiceCollection services, Action<DatabaseOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.TryAddSingleton<DataSources>();
        services.TryAddScoped<SaveChangesGate>();
        services.TryAddScoped<ModuleDbContextServices>();
        return services;
    }

    /// <summary>
    /// 注册模块的写入上下文与查询上下文，查询处理器经 TQueryDb 接口使用查询上下文
    /// </summary>
    /// <remarks>
    /// 不使用 DbContext 池：过滤器读取的当前用户与数据范围是作用域内的状态，池化实例一旦没有重置就是数据越权（05-03 D2）
    /// </remarks>
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

        // 按入口的来源选连接池，在作用域内第一次使用上下文时确定
        services.AddDbContext<TContext>((provider, options) =>
            AddFrameworkExtensions(options.UseNpgsql(provider.GetRequiredService<DataSources>().Transactions(EntryOrigin(provider)))));
        services.AddDbContext<TQueryContext>((provider, options) =>
            AddFrameworkExtensions(options.UseNpgsql(
                provider.GetRequiredService<DataSources>().Reads(EntryOrigin(provider)),
                npgsql => npgsql.ExecutionStrategy(dependencies => new QueryRetryStrategy(dependencies)))));
        services.AddScoped<TQueryDb>(provider => provider.GetRequiredService<TQueryContext>());
        return services;
    }

    private static void AddFrameworkExtensions(DbContextOptionsBuilder options) =>
        ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(new KmicFunctionsExtension());

    private static CallOrigin EntryOrigin(IServiceProvider provider) => provider.GetRequiredService<ExecutionScope>().EntryOrigin;
}

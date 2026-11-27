using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Background;

/// <summary>占用队列 DI 注册</summary>
public static class LeasedWorkServiceCollectionExtensions
{
    /// <summary>注册占用队列并托管其领取循环</summary>
    /// <remarks>队列以单例注册，访问数据库须通过 IExecutionScopeFactory 创建作用域</remarks>
    public static IServiceCollection AddKmicLeasedWork<TWork, TItem>(this IServiceCollection services)
        where TWork : class, ILeasedWork<TItem>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ILeasedWork<TItem>, TWork>();
        services.AddHostedService<LeasedQueueRunner<TItem>>();
        return services;
    }
}

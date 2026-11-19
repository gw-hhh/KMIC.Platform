using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Background;

/// <summary>
/// 登记领取队列
/// </summary>
public static class LeasedWorkServiceCollectionExtensions
{
    /// <summary>
    /// 登记一个队列，由框架运行领取循环
    /// </summary>
    /// <remarks>
    /// 队列是单例，读写数据库要用 IExecutionScopeFactory 建作用域
    /// </remarks>
    /// <typeparam name="TWork">队列的实现</typeparam>
    /// <typeparam name="TItem">领取到的一条</typeparam>
    public static IServiceCollection AddKmicLeasedWork<TWork, TItem>(this IServiceCollection services)
        where TWork : class, ILeasedWork<TItem>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ILeasedWork<TItem>, TWork>();
        services.AddHostedService<LeasedQueueRunner<TItem>>();
        return services;
    }
}

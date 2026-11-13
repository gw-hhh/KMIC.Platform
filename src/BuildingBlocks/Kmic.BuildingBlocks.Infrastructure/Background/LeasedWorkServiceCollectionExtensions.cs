using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Background;

/// <summary>
/// 登记领取队列
/// </summary>
public static class LeasedWorkServiceCollectionExtensions
{
    /// <summary>
    /// 登记一个队列并让框架为它运行领取循环
    /// </summary>
    /// <remarks>
    /// 在模块的 AddWorkerServices 里调用，所以只有带 Worker 角色的实例才有这个循环。队列类型登记为单例，
    /// 要读写数据库就用 IExecutionScopeFactory 建作用域；同一个条目类型只能登记一个队列
    /// </remarks>
    /// <typeparam name="TWork">队列的实现</typeparam>
    /// <typeparam name="TItem">领取到的一条</typeparam>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddKmicLeasedWork<TWork, TItem>(this IServiceCollection services)
        where TWork : class, ILeasedWork<TItem>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ILeasedWork<TItem>, TWork>();
        services.AddHostedService<LeasedQueueRunner<TItem>>();
        return services;
    }
}

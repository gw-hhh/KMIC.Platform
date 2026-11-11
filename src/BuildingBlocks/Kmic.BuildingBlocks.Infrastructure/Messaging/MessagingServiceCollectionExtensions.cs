using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 集成消息的后台服务注册
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// 注册投递与处理的后台循环，只在 Worker 角色调用
    /// </summary>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddKmicMessagingWorker(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<MessagingSignals>();
        services.TryAddSingleton<MessagingListener>();
        services.AddHostedService<MessagingWorker>();
        return services;
    }
}

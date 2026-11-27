using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>消息与后台作业 DI 注册</summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>注册消息投递与消费循环，仅在 Worker 角色调用（须先调用 AddKmicStartupTasks）</summary>
    public static IServiceCollection AddKmicMessagingWorker(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        AddListener(services);
        services.AddHostedService<MessagingWorker>();
        return services;
    }

    /// <summary>注册后台作业执行器，仅在 Worker 角色调用（须先调用 AddKmicStartupTasks）</summary>
    public static IServiceCollection AddKmicJobWorker(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        AddListener(services);
        services.TryAddScoped<IJobHandler<ReadModelRebuildJob>, ReadModelRebuildHandler>();
        services.AddHostedService<JobWorker>();
        return services;
    }

    /// <summary>注册容量保护采样器及状态缓存（容量由 IMessagingCapacitySource 提供，未注册时不判定）</summary>
    public static IServiceCollection AddKmicMessagingAdmission(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<StartupState>();
        services.TryAddSingleton<MessagingAdmission>();
        services.TryAddSingleton<AdmissionSampler>();
        services.AddHostedService(provider => provider.GetRequiredService<AdmissionSampler>());
        return services;
    }

    // 投递、处理与作业执行复用同一数据库监听连接，按幂等注册确保全局唯一
    internal static void AddListener(IServiceCollection services)
    {
        services.TryAddSingleton<StartupState>();
        services.TryAddSingleton<MessagingSignals>();
        services.TryAddSingleton<MessagingListener>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, MessagingListenerService>());
    }
}

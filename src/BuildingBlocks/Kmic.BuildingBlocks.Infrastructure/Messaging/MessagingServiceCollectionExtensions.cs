using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 后台处理的服务注册
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// 注册投递与处理的后台循环，只在 Worker 角色调用
    /// </summary>
    /// <remarks>
    /// 宿主要先调用 AddKmicStartupTasks
    /// </remarks>
    public static IServiceCollection AddKmicMessagingWorker(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        AddListener(services);
        services.AddHostedService<MessagingWorker>();
        return services;
    }

    /// <summary>
    /// 注册后台作业的执行器，只在 Worker 角色调用
    /// </summary>
    /// <remarks>
    /// 宿主要先调用 AddKmicStartupTasks
    /// </remarks>
    public static IServiceCollection AddKmicJobWorker(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        AddListener(services);
        services.TryAddScoped<IJobHandler<ReadModelRebuildJob>, ReadModelRebuildHandler>();
        services.AddHostedService<JobWorker>();
        return services;
    }

    /// <summary>
    /// 注册容量保护：判断数据库空间的采样器（Api 与 Worker 角色都要）和本实例的状态
    /// </summary>
    /// <remarks>
    /// 容量来自 <see cref="IMessagingCapacitySource" />，没有登记时不判断
    /// </remarks>
    public static IServiceCollection AddKmicMessagingAdmission(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<StartupState>();
        services.TryAddSingleton<MessagingAdmission>();
        services.TryAddSingleton<AdmissionSampler>();
        services.AddHostedService(provider => provider.GetRequiredService<AdmissionSampler>());
        return services;
    }

    // 投递、处理和作业执行共用一个监听连接，谁先注册都只注册一次
    internal static void AddListener(IServiceCollection services)
    {
        services.TryAddSingleton<StartupState>();
        services.TryAddSingleton<MessagingSignals>();
        services.TryAddSingleton<MessagingListener>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, MessagingListenerService>());
    }
}

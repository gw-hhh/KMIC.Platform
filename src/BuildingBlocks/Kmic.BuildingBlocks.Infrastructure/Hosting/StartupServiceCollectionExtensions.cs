using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>
/// 启动任务注册
/// </summary>
public static class StartupServiceCollectionExtensions
{
    /// <summary>
    /// 注册启动任务的运行器和框架自带的启动任务，Api、Worker 两种角色都要调用
    /// </summary>
    /// <remarks>
    /// 要在后台组件之前调用：宿主按注册顺序启动
    /// </remarks>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddKmicStartupTasks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<StartupState>();
        services.AddHostedService<StartupTaskRunner>();
        services.AddScoped<IStartupTask, RegisterMessageRoutesTask>();
        services.AddScoped<IStartupTask, QueueReadModelRebuildsTask>();
        return services;
    }
}

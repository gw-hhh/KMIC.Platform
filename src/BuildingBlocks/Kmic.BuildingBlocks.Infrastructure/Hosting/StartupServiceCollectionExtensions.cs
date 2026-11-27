using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>启动任务 DI 注册</summary>
public static class StartupServiceCollectionExtensions
{
    /// <summary>注册启动任务运行器及框架内置启动任务（Api 与 Worker 均须调用）</summary>
    /// <remarks>须在注册后台处理组件之前调用</remarks>
    public static IServiceCollection AddKmicStartupTasks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<StartupState>();
        services.AddHostedService<StartupTaskRunner>();
        services.AddScoped<IStartupTask, MigrationCheckTask>();
        services.AddScoped<IStartupTask, RegisterMessageRoutesTask>();
        services.AddScoped<IStartupTask, QueueReadModelRebuildsTask>();
        return services;
    }
}

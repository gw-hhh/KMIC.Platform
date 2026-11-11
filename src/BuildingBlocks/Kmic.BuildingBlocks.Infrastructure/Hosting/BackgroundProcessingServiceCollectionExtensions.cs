using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>
/// 按角色注册后台处理
/// </summary>
public static class BackgroundProcessingServiceCollectionExtensions
{
    /// <summary>
    /// 停机时等待进行中工作的总时长，宿主设到 HostOptions.ShutdownTimeout。容器的停止宽限期要比它长，部署模板里写 60 秒
    /// </summary>
    public static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(45);

    /// <summary>
    /// 读取配置的角色，如 "Api,Worker"
    /// </summary>
    /// <exception cref="InvalidOperationException">没有配置或含有不认识的角色，宿主拒绝启动</exception>
    public static HostRoles ParseRoles(string? configured)
    {
        var roles = HostRoles.None;
        foreach (var name in (configured ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            roles |= name switch
            {
                "Api" => HostRoles.Api,
                "Worker" => HostRoles.Worker,
                _ => throw new InvalidOperationException($"Kmic:Roles 里的“{name}”不认识，只能是 Api、Worker。"),
            };
        }

        return roles == HostRoles.None ? throw new InvalidOperationException("没有配置 Kmic:Roles，至少要有 Api 或 Worker 之一。") : roles;
    }

    /// <summary>
    /// 注册启动任务、调度器和本实例角色需要的后台组件
    /// </summary>
    /// <remarks>
    /// 两种角色都执行启动任务、注册调度器（只有 Worker 真正触发）；投递、处理、作业执行和定时任务只在 Worker 上运行。
    /// 只做 Api 的实例不建流式池
    /// </remarks>
    /// <param name="services">服务集合</param>
    /// <param name="roles">本实例的角色</param>
    public static IServiceCollection AddKmicBackgroundProcessing(this IServiceCollection services, HostRoles roles)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (roles == HostRoles.None)
        {
            throw new ArgumentException("至少要有 Api 或 Worker 之一。", nameof(roles));
        }

        services.AddKmicStartupTasks();
        services.AddKmicScheduler();
        if (roles.HasFlag(HostRoles.Worker))
        {
            services.AddKmicMessagingWorker();
            services.AddKmicJobWorker();
            services.AddKmicSchedulerWorker();
        }
        else
        {
            services.Configure<DatabaseOptions>(o => o.Worker.StreamMaxPoolSize = 0);
        }

        return services;
    }
}

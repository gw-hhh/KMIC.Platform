using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Observability;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.SecurityEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>后台处理组件 DI 注册</summary>
public static class BackgroundProcessingServiceCollectionExtensions
{
    /// <summary>停机等待在途任务完成的超时时长（须赋给 HostOptions.ShutdownTimeout，容器停止宽限期应大于此值）</summary>
    public static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(45);

    /// <summary>解析实例角色配置（如 "Api,Worker"），未配置或包含未知角色时拒绝启动</summary>
    public static HostRoles ParseRoles(string? configured)
    {
        var roles = HostRoles.None;
        foreach (var name in (configured ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            roles |= name switch
            {
                "Api" => HostRoles.Api,
                "Worker" => HostRoles.Worker,
                _ => throw new InvalidOperationException($"无法识别 Kmic:Roles 中的“{name}”，仅支持 Api 或 Worker。"),
            };
        }

        return roles == HostRoles.None ? throw new InvalidOperationException("未配置 Kmic:Roles，至少须包含 Api 或 Worker 之一。") : roles;
    }

    /// <summary>注册启动任务、调度器及对应实例角色的后台处理组件</summary>
    public static IServiceCollection AddKmicBackgroundProcessing(this IServiceCollection services, HostRoles roles)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (roles == HostRoles.None)
        {
            throw new ArgumentException("至少须包含 Api 或 Worker 之一。", nameof(roles));
        }

        services.AddKmicStartupTasks();
        services.AddKmicSecurityEvents();
        services.AddKmicBacklogSampling();
        services.AddKmicMessagingAdmission();
        services.AddSingleton(new InstanceIdentity(Guid.CreateVersion7(), Environment.MachineName, roles.ToString()));
        services.AddHostedService<InstanceHeartbeat>();
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

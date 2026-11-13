using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Hosting.Health;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Presentation;
using Kmic.BuildingBlocks.Presentation.Authentication;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Kmic.BuildingBlocks.Hosting;

/// <summary>
/// 宿主的组装
/// </summary>
public static class KmicHostExtensions
{
    private const string ReadyTag = "ready";

    /// <summary>
    /// 按配置的角色装配框架服务，并注册要运行的模块
    /// </summary>
    /// <remarks>
    /// 角色在 Kmic:Roles：Api 接 HTTP 请求，Worker 处理消息、执行作业与定时任务，可以同时有。
    /// 管道行为要由宿主在 AddMediator 里按顺序声明，源码生成器要求这样写，这里不替宿主注册。
    /// 校验失败（模块配对、依赖、角色、数据库连接串）都拒绝启动，问题一次报全
    /// </remarks>
    /// <param name="builder">宿主构建器</param>
    /// <param name="configure">登记模块</param>
    public static WebApplicationBuilder AddKmicHost(this WebApplicationBuilder builder, Action<KmicHostBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var roles = BackgroundProcessingServiceCollectionExtensions.ParseRoles(builder.Configuration["Kmic:Roles"]);
        var host = new KmicHostBuilder();
        configure(host);
        var modules = ModuleSelection.Select(host.Modules, builder.Environment, builder.Configuration);
        if (modules.Count == 0)
        {
            throw new DeclarationException("模块注册", ["没有启用任何模块。"]);
        }

        var services = builder.Services;
        services.Configure<HostOptions>(options => options.ShutdownTimeout = BackgroundProcessingServiceCollectionExtensions.ShutdownTimeout);
        services.AddSingleton(new HostComposition(roles, [.. modules.Select(m => m.Module.Name)]));

        services.AddKmicApplication([.. modules.Select(m => m.Module.ApplicationAssembly)]);
        services.AddOptions<DatabaseOptions>()
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), $"{DatabaseOptions.Section}:ConnectionString 没有配置。")
            .ValidateOnStart();
        services.AddKmicPersistence(options =>
        {
            options.AutoMigrate = builder.Environment.IsDevelopment();
            builder.Configuration.GetSection(DatabaseOptions.Section).Bind(options);
        });
        foreach (var module in modules.Select(m => m.Module))
        {
            module.AddServices(services);
            if (roles.HasFlag(HostRoles.Worker))
            {
                module.AddWorkerServices(services);
            }

            services.AddKmicSubscriptions(module.Name, module.ConfigureMessaging);
            services.AddKmicScheduling(module.Name, module.ConfigureScheduling);
        }

        services.AddKmicFrameworkOptions();
        services.AddKmicBackgroundProcessing(roles);
        if (roles.HasFlag(HostRoles.Api))
        {
            services.AddKmicPresentation();
            services.AddKmicAuthentication();
            foreach (var endpoints in modules.Select(m => m.Endpoints))
            {
                services.AddSingleton(endpoints);
            }

            // 没有访问上下文加载器，所有请求都无法确定是谁：由 Identity 模块提供
            services.AddTransient<IStartupFilter, AccessContextLoaderFilter>();
        }

        AddHealthChecks(services, roles);
        return builder;
    }

    /// <summary>
    /// 搭建请求管道并映射端点与健康检查
    /// </summary>
    /// <remarks>
    /// Api 角色的顺序：错误处理 → 路由 → 认证 → 访问上下文 → 限流与并发限制 → 授权 → 端点。
    /// 只跑 Worker 的实例也暴露健康检查，供编排平台判断存活，但不接业务流量
    /// </remarks>
    public static WebApplication UseKmicHost(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var composition = app.Services.GetRequiredService<HostComposition>();
        var api = composition.Roles.HasFlag(HostRoles.Api);
        if (api)
        {
            app.UseKmicErrorHandling();
        }

        app.UseRouting();
        if (api)
        {
            app.UseAuthentication();
            app.UseKmicAccessContext();
            app.UseKmicLimits();
            app.UseAuthorization();
            app.MapKmicEndpoints();
        }

        // 存活只看进程能不能响应；就绪见 ReadinessHealthCheck。都匿名、只返回 Healthy / Unhealthy
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = registration => registration.Tags.Contains(ReadyTag) });
        DependencyEndpoint.Map(app);
        return app;
    }

    // 就绪只放“没有它整个服务就无法工作”的条件；依赖检查列出全部依赖，模块要加自己的依赖就用 DependencyHealth.Tag 登记
    private static void AddHealthChecks(IServiceCollection services, HostRoles roles)
    {
        services.AddSingleton<DatabaseProbe>();
        var checks = services.AddHealthChecks()
            .AddCheck<ReadinessHealthCheck>("ready", tags: [ReadyTag])
            .AddCheck<DatabaseDependencyCheck>("database", tags: [DependencyHealth.Tag], timeout: DatabaseProbe.Timeout)
            .AddCheck<ListenerDependencyCheck>("message-listener", tags: [DependencyHealth.Tag], timeout: DatabaseProbe.Timeout)
            .AddCheck<FileStorageDependencyCheck>("file-storage", tags: [DependencyHealth.Tag], timeout: DatabaseProbe.Timeout);
        if (roles.HasFlag(HostRoles.Worker))
        {
            checks.AddCheck<SchedulerDependencyCheck>("scheduler", tags: [DependencyHealth.Tag], timeout: DatabaseProbe.Timeout);
        }
    }
}

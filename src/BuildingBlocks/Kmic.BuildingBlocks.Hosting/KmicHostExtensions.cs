using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Hosting.Health;
using Kmic.BuildingBlocks.Infrastructure.Features;
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

/// <summary>宿主装配与管道配置扩展。</summary>
public static class KmicHostExtensions
{
    private const string ReadyTag = "ready";

    /// <summary>按配置的角色装配框架服务并注册模块（管道行为由宿主在 AddMediator 中显式声明）。</summary>
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
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), $"{DatabaseOptions.Section}:ConnectionString 未配置。")
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

        services.AddKmicFeatures(builder.Configuration);
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

            // 缺少访问上下文加载器将无法识别请求身份，该实现由 Identity 模块提供
            services.AddTransient<IStartupFilter, AccessContextLoaderFilter>();
        }

        AddHealthChecks(services, roles);
        return builder;
    }

    /// <summary>搭建请求管道并映射端点与健康检查。</summary>
    public static WebApplication UseKmicHost(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var composition = app.Services.GetRequiredService<HostComposition>();
        var api = composition.Roles.HasFlag(HostRoles.Api);
        if (api)
        {
            app.UseKmicForwardedHeaders();
            app.UseKmicSecurityHeaders();
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

        // 存活探针仅检查进程响应能力，就绪探针见 ReadinessHealthCheck；均允许匿名访问且仅返回 Healthy / Unhealthy
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = registration => registration.Tags.Contains(ReadyTag) });
        DependencyEndpoint.Map(app);
        return app;
    }

    // 就绪检查仅包含服务不可用时的必要条件；依赖检查汇总全部依赖项，模块自定义依赖需标记 DependencyHealth.Tag
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

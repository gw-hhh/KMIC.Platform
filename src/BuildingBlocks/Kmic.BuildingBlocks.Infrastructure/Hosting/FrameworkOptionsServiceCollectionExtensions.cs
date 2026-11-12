using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.Time;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Caching;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Maintenance;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Observability;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Kmic.BuildingBlocks.Infrastructure.SecurityEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>
/// 把框架各项配置接到配置文件和环境变量上
/// </summary>
public static class FrameworkOptionsServiceCollectionExtensions
{
    /// <summary>
    /// 框架自己的配置类逐个绑定到各自的配置节（Kmic:Jobs、Kmic:Messaging 等），由宿主调用
    /// </summary>
    /// <remarks>
    /// 各个 AddKmic… 只注册默认值，不读配置：它们也要在没有配置系统的测试里使用。
    /// 宿主调用这个方法之后，部署时才能用配置文件或环境变量改这些值
    /// </remarks>
    /// <param name="services">服务集合，要求已经有 IConfiguration</param>
    public static IServiceCollection AddKmicFrameworkOptions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<PipelineOptions>().BindConfiguration(PipelineOptions.Section);
        services.AddOptions<BusinessTimeOptions>().BindConfiguration(BusinessTimeOptions.Section);
        services.AddOptions<IdempotencyOptions>().BindConfiguration(IdempotencyOptions.Section);
        services.AddOptions<AuditOptions>().BindConfiguration(AuditOptions.Section);
        services.AddOptions<CacheOptions>().BindConfiguration(CacheOptions.Section);
        services.AddOptions<JobOptions>().BindConfiguration(JobOptions.Section);
        services.AddOptions<MessagingOptions>().BindConfiguration(MessagingOptions.Section);
        services.AddOptions<PurgeOptions>().BindConfiguration(PurgeOptions.Section);
        services.AddOptions<SecurityEventOptions>().BindConfiguration(SecurityEventOptions.Section);
        services.AddOptions<DatabaseDiagnosticsOptions>().BindConfiguration(DatabaseDiagnosticsOptions.Section);

        // 多个实例的游标签名密钥必须一致，所以生产环境一定要配置；开发和测试可以不配，每个进程随机一个
        services.AddOptions<CursorOptions>()
            .BindConfiguration(CursorOptions.Section)
            .Validate<IHostEnvironment>(
                (options, environment) => !environment.IsProduction() || !string.IsNullOrWhiteSpace(options.SigningKey),
                "生产环境必须配置 Kmic:Paging:SigningKey（游标签名密钥，Base64，至少 32 字节），多个实例用同一个值")
            .ValidateOnStart();
        return services;
    }
}

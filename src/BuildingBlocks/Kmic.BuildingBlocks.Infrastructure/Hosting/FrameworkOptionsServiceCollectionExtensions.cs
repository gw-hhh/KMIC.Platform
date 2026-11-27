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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>框架配置选项 DI 注册</summary>
public static class FrameworkOptionsServiceCollectionExtensions
{
    /// <summary>绑定框架各配置类到配置节（宿主启动时须调用，否则部署配置不生效）</summary>
    public static IServiceCollection AddKmicFrameworkOptions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<PipelineOptions>().BindConfiguration(PipelineOptions.Section);
        services.AddOptions<BusinessTimeOptions>().BindConfiguration(BusinessTimeOptions.Section);
        services.AddOptions<IdempotencyOptions>().BindConfiguration(IdempotencyOptions.Section);
        services.AddOptions<AuditOptions>().BindConfiguration(AuditOptions.Section);
        services.AddOptions<CacheOptions>().BindConfiguration(CacheOptions.Section);
        services.AddOptions<JobOptions>().BindConfiguration(JobOptions.Section);
        services.AddOptions<MessagingOptions>().BindConfiguration(MessagingOptions.Section).ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<MessagingOptions>, MessagingOptionsValidator>());
        services.AddOptions<PurgeOptions>().BindConfiguration(PurgeOptions.Section);
        services.AddOptions<SecurityEventOptions>().BindConfiguration(SecurityEventOptions.Section);
        services.AddOptions<DatabaseDiagnosticsOptions>().BindConfiguration(DatabaseDiagnosticsOptions.Section);

        // 多实例间游标签名密钥必须保持一致，生产环境强制配置；开发与测试环境未配置时由各进程随机生成
        services.AddOptions<CursorOptions>()
            .BindConfiguration(CursorOptions.Section)
            .Validate<IHostEnvironment>(
                (options, environment) => !environment.IsProduction() || !string.IsNullOrWhiteSpace(options.SigningKey),
                "生产环境必须配置 Kmic:Paging:SigningKey（游标签名密钥，Base64 编码，至少 32 字节），且多实例间须保持一致")
            .ValidateOnStart();
        return services;
    }
}

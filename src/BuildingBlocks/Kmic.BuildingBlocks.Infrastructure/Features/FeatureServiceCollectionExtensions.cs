using Kmic.BuildingBlocks.Application.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FeatureManagement;

namespace Kmic.BuildingBlocks.Infrastructure.Features;

/// <summary>功能开关 DI 注册</summary>
public static class FeatureServiceCollectionExtensions
{
    /// <summary>配置节名称，开关定义在 Kmic:Features:{开关名}（支持布尔值或 Feature Management 过滤器）</summary>
    public const string Section = "Kmic:Features";

    /// <summary>注册功能开关服务（读取部署配置，变更后需重启生效）</summary>
    public static IServiceCollection AddKmicFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddFeatureManagement(configuration.GetSection(Section));
        services.TryAddScoped<IFeatureFlags, FeatureFlags>();
        return services;
    }
}

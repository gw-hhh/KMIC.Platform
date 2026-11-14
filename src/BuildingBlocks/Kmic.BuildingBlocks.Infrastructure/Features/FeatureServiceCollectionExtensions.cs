using Kmic.BuildingBlocks.Application.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FeatureManagement;

namespace Kmic.BuildingBlocks.Infrastructure.Features;

/// <summary>
/// 功能开关服务注册
/// </summary>
public static class FeatureServiceCollectionExtensions
{
    /// <summary>
    /// 配置节：开关写在 <c>Kmic:Features:{开关名}</c>，值是 true 或 false，也可以用 Feature Management 的过滤器写法
    /// </summary>
    public const string Section = "Kmic:Features";

    /// <summary>
    /// 注册功能开关：值来自配置，改了要重启生效
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置根</param>
    public static IServiceCollection AddKmicFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // 开关名里有点（Sample.NewImport）：Feature Management 要求配置节里每个开关是一个子节，名字含点时按整体当作名字
        services.AddFeatureManagement(configuration.GetSection(Section));
        services.TryAddScoped<IFeatureFlags, FeatureFlags>();
        return services;
    }
}

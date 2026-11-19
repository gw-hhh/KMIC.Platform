using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// 缓存的服务注册
/// </summary>
public static class CacheServiceCollectionExtensions
{
    private const string InnerCacheKey = "Kmic.Cache.Inner";

    /// <summary>
    /// 注册 HybridCache 及其命中统计、提交后失效和跨实例的失效通知
    /// </summary>
    /// <remarks>
    /// 业务代码只用 HybridCache。没有配置 Redis 时只有本地缓存。失效通知走数据库的监听连接，
    /// 只做 Api 的实例也要监听，所以这里也注册监听
    /// </remarks>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddKmicCache(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<CacheOptions>();
        services.AddHybridCache();
        DecorateWithMetrics(services);
        services.AddOptions<HybridCacheOptions>().Configure<IOptions<CacheOptions>>((hybrid, cache) =>
        {
            hybrid.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                LocalCacheExpiration = cache.Value.DefaultLocalExpiration,
                Expiration = cache.Value.DefaultLocalExpiration,
            };
        });
        services.TryAddSingleton<CacheInvalidator>();
        MessagingServiceCollectionExtensions.AddListener(services);
        return services;
    }

    // 原注册改成带键服务，仍由容器创建和释放，对外的 HybridCache 换成带命中统计的包装；已经包过就不再包
    private static void DecorateWithMetrics(IServiceCollection services)
    {
        if (services.Any(d => d.ServiceType == typeof(HybridCache) && d.IsKeyedService && Equals(d.ServiceKey, InnerCacheKey)))
        {
            return;
        }

        var original = services.Last(d => d.ServiceType == typeof(HybridCache) && !d.IsKeyedService);
        services.Remove(original);
        services.Add(original.ImplementationType is { } type
            ? ServiceDescriptor.KeyedSingleton(typeof(HybridCache), InnerCacheKey, type)
            : ServiceDescriptor.KeyedSingleton(typeof(HybridCache), InnerCacheKey, (provider, _) => original.ImplementationFactory?.Invoke(provider) ?? original.ImplementationInstance!));
        services.AddSingleton<HybridCache>(provider => new MeteredHybridCache(
            provider.GetRequiredKeyedService<HybridCache>(InnerCacheKey), provider.GetRequiredService<FrameworkMetrics>()));
    }
}

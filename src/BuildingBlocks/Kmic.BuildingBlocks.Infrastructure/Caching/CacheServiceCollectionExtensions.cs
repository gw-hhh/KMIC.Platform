using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Caching;

/// <summary>缓存 DI 注册</summary>
public static class CacheServiceCollectionExtensions
{
    private const string InnerCacheKey = "Kmic.Cache.Inner";

    /// <summary>注册 HybridCache 及其命中统计和失效广播</summary>
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

    // 将原 HybridCache 转为键控服务由容器管理生命周期，再注册带命中统计的包装器（幂等）
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

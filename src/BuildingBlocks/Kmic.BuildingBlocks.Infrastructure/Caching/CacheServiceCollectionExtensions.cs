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
    /// <summary>
    /// 注册 HybridCache、提交后失效和跨实例的失效通知
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
}

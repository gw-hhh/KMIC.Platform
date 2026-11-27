using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Application;
using Kmic.Configuration.Domain.Settings;
using Kmic.Configuration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Kmic.Configuration.Infrastructure.Settings;

// 整库缓存已保存的系统设置并按标签失效，缺失时使用代码默认值
internal sealed class SettingsReader(IQueryScopeFactory queries, HybridCache cache, SystemSettingCatalog catalog) : ISettings
{
    // 定期过期，避免失效通知丢失后一直读取旧值
    private static readonly HybridCacheEntryOptions _entry = new()
    {
        LocalCacheExpiration = TimeSpan.FromMinutes(10),
        Expiration = TimeSpan.FromHours(1),
    };

    public async ValueTask<T> GetAsync<T>(CancellationToken ct)
        where T : class, ISystemSetting<T>
    {
        var definition = catalog.Get<T>();
        var snapshot = await cache.GetOrCreateAsync(ConfigurationCacheKeys.SettingsTag, queries, LoadAsync, _entry, tags: [ConfigurationCacheKeys.SettingsTag], cancellationToken: ct);
        return snapshot.Get<T>(definition.Key);
    }

    private static async ValueTask<SettingsSnapshot> LoadAsync(IQueryScopeFactory queries, CancellationToken ct) =>
        await queries.RunAsync<ConfigurationQueryDbContext, SettingsSnapshot>(
            CallOrigin.ModuleCall,
            async (db, token) => new SettingsSnapshot(await db.Set<SystemSettingValue>().AsNoTracking().ToDictionaryAsync(v => v.Key, v => v.Value, StringComparer.Ordinal, token)),
            ct);
}

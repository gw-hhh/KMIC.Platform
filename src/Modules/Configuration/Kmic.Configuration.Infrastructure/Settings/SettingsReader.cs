using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Application;
using Kmic.Configuration.Domain.Settings;
using Kmic.Configuration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Kmic.Configuration.Infrastructure.Settings;

/// <summary>
/// <see cref="ISettings" /> 的实现：全部被修改过的值放在一个缓存项里，按标签失效；没有被修改过的设置用代码里的默认值
/// </summary>
internal sealed class SettingsReader(IQueryScopeFactory queries, HybridCache cache, SystemSettingCatalog catalog) : ISettings
{
    // 正常靠标签失效；过期时间只是失效广播万一丢了的兜底
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

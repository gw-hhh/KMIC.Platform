using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Application;
using Kmic.Configuration.Application.Dictionaries;
using Kmic.Configuration.Contracts;
using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Kmic.Configuration.Infrastructure.Dictionaries;

/// <summary>
/// 读字典快照：一个类型一个缓存项，键与标签都是 <see cref="ConfigurationCacheKeys.Dictionary" />，管理员改动提交之后按标签失效
/// </summary>
internal sealed class DictionarySnapshotReader(IQueryScopeFactory queries, HybridCache cache) : IDictionarySnapshots
{
    // 正常靠标签失效；过期时间只是失效广播万一丢了的兜底
    private static readonly HybridCacheEntryOptions _entry = new()
    {
        LocalCacheExpiration = TimeSpan.FromMinutes(10),
        Expiration = TimeSpan.FromHours(1),
    };

    public async Task<DictionarySnapshot?> GetAsync(string typeCode, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeCode);

        var key = ConfigurationCacheKeys.Dictionary(typeCode);
        return await cache.GetOrCreateAsync(key, (queries, typeCode), LoadAsync, _entry, tags: [ConfigurationCacheKeys.DictionaryTag, key], cancellationToken: ct);
    }

    private static async ValueTask<DictionarySnapshot?> LoadAsync((IQueryScopeFactory Queries, string TypeCode) state, CancellationToken ct) =>
        await state.Queries.RunAsync<ConfigurationQueryDbContext, DictionarySnapshot?>(
            CallOrigin.ModuleCall,
            async (db, token) =>
            {
                var type = await db.Set<DictionaryType>().AsNoTracking().Include(t => t.Items).SingleOrDefaultAsync(t => t.Code == state.TypeCode, token);
                return type is null
                    ? null
                    : new DictionarySnapshot(
                        type.Code,
                        type.DisplayName,
                        type.IsHierarchical,
                        type.ConcurrencyVersion,
                        [
                            .. type.Items
                                .OrderBy(i => i.ParentCode, StringComparer.Ordinal)
                                .ThenBy(i => i.SortOrder)
                                .ThenBy(i => i.Code, StringComparer.Ordinal)
                                .Select(i => new DictionaryEntry(i.Code, i.Name, i.ParentCode, i.SortOrder, i.Status == DictionaryItemStatus.Active)),
                        ]);
            },
            ct);
}

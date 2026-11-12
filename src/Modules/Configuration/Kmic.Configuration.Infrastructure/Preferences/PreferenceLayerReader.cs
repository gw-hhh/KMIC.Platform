using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Application;
using Kmic.Configuration.Application.Preferences;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Configuration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Kmic.Configuration.Infrastructure.Preferences;

/// <summary>
/// <see cref="IPreferenceLayers" /> 的实现：系统层、每个角色层各占一个缓存项，按标签失效；用户层每次直接读库
/// </summary>
/// <remarks>
/// 用户层不缓存：每个用户只在启动时读一次，缓存收益很小，却要为每个用户记一份、改一次失效一次
/// </remarks>
internal sealed class PreferenceLayerReader(IQueryScopeFactory queries, HybridCache cache) : IPreferenceLayers
{
    // 正常靠标签失效；过期时间只是失效广播万一丢了的兜底
    private static readonly HybridCacheEntryOptions _entry = new()
    {
        LocalCacheExpiration = TimeSpan.FromMinutes(10),
        Expiration = TimeSpan.FromHours(1),
    };

    private static readonly IReadOnlyDictionary<string, string> _none = new Dictionary<string, string>(StringComparer.Ordinal);

    public async Task<LayeredPreferences> GetAsync(UserId? userId, RoleId? roleId, CancellationToken ct)
    {
        var system = await LayerAsync(ConfigurationCacheKeys.PreferencesSystem, PreferenceLayer.System, null, ct);
        if (roleId is not { } role)
        {
            return new LayeredPreferences(system.Values, _none, _none);
        }

        var byRole = await LayerAsync(ConfigurationCacheKeys.PreferencesRole(role), PreferenceLayer.Role, role, ct);
        var mine = userId is { } user ? await UserLayerAsync(user, role, ct) : _none;
        return new LayeredPreferences(system.Values, byRole.Values, mine);
    }

    private async Task<LayerSnapshot> LayerAsync(string tag, PreferenceLayer layer, RoleId? roleId, CancellationToken ct) =>
        await cache.GetOrCreateAsync(
            tag,
            (queries, layer, roleId),
            static async (state, token) => new LayerSnapshot(await state.queries.RunAsync<ConfigurationQueryDbContext, Dictionary<string, string>>(
                CallOrigin.ModuleCall,
                async (db, inner) => await db.Set<PreferenceValue>().AsNoTracking()
                    .Where(v => v.Layer == state.layer && v.RoleId == state.roleId)
                    .ToDictionaryAsync(v => v.Key, v => v.Value, StringComparer.Ordinal, inner),
                token)),
            _entry,
            tags: [tag],
            cancellationToken: ct);

    private async Task<Dictionary<string, string>> UserLayerAsync(UserId userId, RoleId roleId, CancellationToken ct) =>
        await queries.RunAsync<ConfigurationQueryDbContext, Dictionary<string, string>>(
            CallOrigin.ModuleCall,
            async (db, token) => await db.Set<PreferenceValue>().AsNoTracking()
                .Where(v => v.Layer == PreferenceLayer.UserRole && v.RoleId == roleId && v.UserId == userId)
                .ToDictionaryAsync(v => v.Key, v => v.Value, StringComparer.Ordinal, token),
            ct);

    /// <summary>
    /// 一层的全部值，进缓存，不可变
    /// </summary>
    [ImmutableObject(true)]
    private sealed class LayerSnapshot(Dictionary<string, string> values)
    {
        public IReadOnlyDictionary<string, string> Values { get; } = values;
    }
}

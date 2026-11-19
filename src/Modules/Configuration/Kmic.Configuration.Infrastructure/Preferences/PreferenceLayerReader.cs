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
/// 读取分层偏好：系统层和各角色层分别缓存并按标签失效，用户层直接读库
/// </summary>
internal sealed class PreferenceLayerReader(IQueryScopeFactory queries, HybridCache cache) : IPreferenceLayers
{
    // 定期过期，避免失效通知丢失后一直读取旧值
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
    /// 单层偏好的缓存快照，内容不可变
    /// </summary>
    [ImmutableObject(true)]
    private sealed class LayerSnapshot(Dictionary<string, string> values)
    {
        public IReadOnlyDictionary<string, string> Values { get; } = values;
    }
}

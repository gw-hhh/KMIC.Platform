using Kmic.BuildingBlocks.Application.Settings;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>用户在当前角色下的三层偏好值，按个人、角色、系统优先级顺序取值</summary>
public sealed class LayeredPreferences(
    IReadOnlyDictionary<string, string> system,
    IReadOnlyDictionary<string, string> role,
    IReadOnlyDictionary<string, string> user)
{
    /// <summary>任一层级已设值的全部键</summary>
    public IEnumerable<string> Keys => user.Keys.Concat(role.Keys).Concat(system.Keys).Distinct(StringComparer.Ordinal);

    /// <summary>按优先级返回指定键在允许层级中的候选值，由调用方选用首个解析和校验通过的值</summary>
    public IEnumerable<(PreferenceSource Source, string Json)> Candidates(string key, PreferenceLayers allowed)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (allowed.HasFlag(PreferenceLayers.UserRole) && user.TryGetValue(key, out var mine))
        {
            yield return (PreferenceSource.UserRole, mine);
        }

        if (allowed.HasFlag(PreferenceLayers.Role) && role.TryGetValue(key, out var byRole))
        {
            yield return (PreferenceSource.Role, byRole);
        }

        if (allowed.HasFlag(PreferenceLayers.System) && system.TryGetValue(key, out var bySystem))
        {
            yield return (PreferenceSource.System, bySystem);
        }
    }
}

using Kmic.BuildingBlocks.Application.Settings;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>
/// 一个用户在当前角色下能看到的三层偏好值：键到 JSON
/// </summary>
/// <remarks>
/// 按“本人 → 角色 → 系统”的顺序取值。偏好声明里不允许的层，即使库里有值也不取
/// </remarks>
public sealed class LayeredPreferences(
    IReadOnlyDictionary<string, string> system,
    IReadOnlyDictionary<string, string> role,
    IReadOnlyDictionary<string, string> user)
{
    /// <summary>
    /// 任何一层上有值的全部键
    /// </summary>
    public IEnumerable<string> Keys => user.Keys.Concat(role.Keys).Concat(system.Keys).Distinct(StringComparer.Ordinal);

    /// <summary>
    /// 键在各个允许的层上的值，按优先级由高到低；调用方取第一个读得出来的
    /// </summary>
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

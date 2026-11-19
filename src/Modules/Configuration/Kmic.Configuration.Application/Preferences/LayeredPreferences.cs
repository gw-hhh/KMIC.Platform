using Kmic.BuildingBlocks.Application.Settings;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>
/// 用户在当前角色下的三层偏好，按键保存 JSON 值
/// </summary>
/// <remarks>
/// 按本人、角色、系统的顺序取值
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
    /// 按优先级返回指定键在允许层级中的值，调用方选择首个解析和校验成功的值
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

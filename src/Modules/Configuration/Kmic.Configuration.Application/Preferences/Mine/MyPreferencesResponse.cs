namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 本人的偏好
/// </summary>
/// <param name="Preferences">按键排序</param>
public sealed record MyPreferencesResponse(IReadOnlyList<MyPreferenceItem> Preferences);

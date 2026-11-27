namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>个人偏好响应（按键排序）</summary>
public sealed record MyPreferencesResponse(IReadOnlyList<MyPreferenceItem> Preferences);

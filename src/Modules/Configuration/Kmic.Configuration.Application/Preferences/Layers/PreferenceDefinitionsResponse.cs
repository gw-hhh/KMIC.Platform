namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>偏好管理清单（普通偏好在前，偏好族在后）</summary>
public sealed record PreferenceDefinitionsResponse(IReadOnlyList<PreferenceDefinitionItem> Preferences);

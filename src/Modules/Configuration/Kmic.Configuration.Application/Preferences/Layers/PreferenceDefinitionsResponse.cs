namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 偏好清单
/// </summary>
/// <param name="Preferences">普通偏好在前、偏好族在后，各组按模块和键排序</param>
public sealed record PreferenceDefinitionsResponse(IReadOnlyList<PreferenceDefinitionItem> Preferences);

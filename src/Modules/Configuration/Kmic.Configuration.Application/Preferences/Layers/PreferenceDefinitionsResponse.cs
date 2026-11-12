namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 偏好清单
/// </summary>
/// <param name="Preferences">按所属模块、键排序</param>
public sealed record PreferenceDefinitionsResponse(IReadOnlyList<PreferenceDefinitionItem> Preferences);

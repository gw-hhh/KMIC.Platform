using System.Text.Json;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>偏好管理列表项</summary>
/// <param name="IsFamily">是否为偏好族键</param>
/// <param name="SystemLayer">系统层已保存的值，未设置时为空</param>
/// <param name="RoleLayers">各角色层已保存的值</param>
public sealed record PreferenceDefinitionItem(
    string Key,
    string DisplayName,
    string? Module,
    bool IsFamily,
    IReadOnlyList<PreferenceLayer> Layers,
    JsonElement? DefaultValue,
    PreferenceLayerValue? SystemLayer,
    IReadOnlyList<PreferenceLayerValue> RoleLayers);

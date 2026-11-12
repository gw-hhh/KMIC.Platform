using System.Text.Json;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 管理页上的一个偏好
/// </summary>
/// <param name="Key">键</param>
/// <param name="DisplayName">显示名称</param>
/// <param name="Module">所属模块；偏好族里的键属于框架，为空</param>
/// <param name="IsFamily">是否偏好族里的键</param>
/// <param name="Layers">允许设置的层，优先级由高到低</param>
/// <param name="DefaultValue">代码里的默认值；偏好族里的键没有</param>
/// <param name="SystemLayer">系统层现有的值，没有设置过为空</param>
/// <param name="RoleLayers">各角色层现有的值</param>
public sealed record PreferenceDefinitionItem(
    string Key,
    string DisplayName,
    string? Module,
    bool IsFamily,
    IReadOnlyList<PreferenceLayer> Layers,
    JsonElement? DefaultValue,
    PreferenceLayerValue? SystemLayer,
    IReadOnlyList<PreferenceLayerValue> RoleLayers);

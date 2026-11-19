using System.Text;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>
/// 可读写的偏好，来自目录声明或偏好族
/// </summary>
/// <param name="Key">键</param>
/// <param name="DisplayName">显示名称</param>
/// <param name="Layers">允许设置的层</param>
/// <param name="Definition">目录里的定义</param>
internal sealed record PreferenceSubject(string Key, string DisplayName, PreferenceLayers Layers, PreferenceDefinition? Definition)
{
    /// <summary>
    /// 是否偏好族里的键
    /// </summary>
    public bool IsFamily => Definition is null;

    /// <summary>
    /// 按键查找偏好，优先匹配目录声明，其次匹配偏好族
    /// </summary>
    public static Result<PreferenceSubject> Find(PreferenceCatalog catalog, string key)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(key);

        if (catalog.Find(key) is { } definition)
        {
            return new PreferenceSubject(definition.Key, definition.DisplayName, definition.Layers, definition);
        }

        return PreferenceFamilies.Find(key) is { } family
            ? new PreferenceSubject(key, family.DisplayName, PreferenceLayers.All, null)
            : ConfigurationErrors.PreferenceNotFound;
    }

    /// <summary>
    /// 是否允许在指定层设置
    /// </summary>
    public Result CheckLayer(PreferenceLayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);

        return Layers.HasFlag(ToFlag(layer)) ? Result.Success() : ConfigurationErrors.PreferenceLayerNotAllowed;
    }

    /// <summary>
    /// 校验并生成存储用的 JSON；已声明偏好按类型校验，偏好族只检查对象格式和大小
    /// </summary>
    public Result<string> Normalize(JsonElement value, int familyMaxKb)
    {
        if (Definition is { } definition)
        {
            var parsed = DeclaredValues.Parse(definition, value, ConfigurationErrors.PreferenceInvalid, ConfigurationErrors.PreferenceInvalidWith);
            return parsed.IsFailure ? parsed.Error : DeclaredValues.ToJson(definition, parsed.Value);
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            return ConfigurationErrors.PreferenceInvalid.WithDetail("值必须是 JSON 对象");
        }

        var json = value.GetRawText();
        return Encoding.UTF8.GetByteCount(json) > familyMaxKb * 1024
            ? ConfigurationErrors.PreferenceInvalid.WithDetail($"偏好值不能超过 {familyMaxKb} KB")
            : json;
    }

    /// <summary>
    /// 允许的层，按优先级由高到低
    /// </summary>
    public IReadOnlyList<PreferenceLayer> LayerList() =>
        [.. PreferenceLayer.All.Where(l => Layers.HasFlag(ToFlag(l))).OrderByDescending(ToFlag)];

    private static PreferenceLayers ToFlag(PreferenceLayer layer)
    {
        if (layer == PreferenceLayer.System)
        {
            return PreferenceLayers.System;
        }

        return layer == PreferenceLayer.Role ? PreferenceLayers.Role : PreferenceLayers.UserRole;
    }
}

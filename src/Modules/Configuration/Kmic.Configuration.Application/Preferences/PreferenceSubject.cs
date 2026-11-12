using System.Text;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>
/// 一个能读写的偏好：在偏好目录里声明过的，或者属于某个偏好族
/// </summary>
/// <param name="Key">键</param>
/// <param name="DisplayName">显示名称</param>
/// <param name="Layers">允许设置的层</param>
/// <param name="Definition">目录里的定义；偏好族里的键没有</param>
internal sealed record PreferenceSubject(string Key, string DisplayName, PreferenceLayers Layers, PreferenceDefinition? Definition)
{
    /// <summary>
    /// 是否偏好族里的键
    /// </summary>
    public bool IsFamily => Definition is null;

    /// <summary>
    /// 按键找：先看目录，再看偏好族；都不是就是不存在
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
    /// 这个偏好是否允许在这一层设置
    /// </summary>
    public Result CheckLayer(PreferenceLayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);

        return Layers.HasFlag(ToFlag(layer)) ? Result.Success() : ConfigurationErrors.PreferenceLayerNotAllowed;
    }

    /// <summary>
    /// 把调用方给的值整理成要存的 JSON：目录里的偏好按值类型读出并校验，偏好族里的只要求是不超大小的 JSON 对象
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
            ? ConfigurationErrors.PreferenceInvalid.WithDetail($"值太大，不能超过 {familyMaxKb} KB")
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

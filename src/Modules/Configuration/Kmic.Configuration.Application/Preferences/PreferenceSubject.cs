using System.Text;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences;

// 可读写的偏好主体，来自目录声明或偏好族
internal sealed record PreferenceSubject(string Key, string DisplayName, PreferenceLayers Layers, PreferenceDefinition? Definition)
{
    public bool IsFamily => Definition is null;

    // 优先匹配目录声明，其次匹配偏好族
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

    public Result CheckLayer(PreferenceLayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);

        return Layers.HasFlag(ToFlag(layer)) ? Result.Success() : ConfigurationErrors.PreferenceLayerNotAllowed;
    }

    // 已声明偏好按类型校验并重序列化，偏好族只检查 JSON 对象格式与字节上限
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

    // 返回允许设置的层，按优先级由高到低排列
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>系统设置及其默认值、当前值和可用动作</summary>
/// <param name="IsModified">是否已保存自定义值</param>
/// <param name="ConcurrencyVersion">保存记录的版本号，未保存值时为空</param>
/// <param name="EditPermission">修改所需权限点</param>
public sealed record SettingItem(
    string Key,
    string DisplayName,
    string Module,
    string? Group,
    IReadOnlyList<SettingFieldInfo> Fields,
    JsonElement DefaultValue,
    JsonElement CurrentValue,
    bool IsModified,
    [property: JsonPropertyName("version")] long? ConcurrencyVersion,
    string EditPermission,
    IReadOnlyList<string> AllowedActions);

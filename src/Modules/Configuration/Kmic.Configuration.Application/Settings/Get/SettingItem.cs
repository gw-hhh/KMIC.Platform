using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>
/// 系统设置及其默认值、当前值和可用动作
/// </summary>
/// <param name="Key">设置键</param>
/// <param name="DisplayName">显示名</param>
/// <param name="Module">声明它的模块</param>
/// <param name="Group">分组</param>
/// <param name="Fields">值里的成员清单</param>
/// <param name="DefaultValue">代码里的默认值</param>
/// <param name="CurrentValue">管理页当前值</param>
/// <param name="IsModified">是否已保存自定义值</param>
/// <param name="ConcurrencyVersion">保存记录的版本号，未保存值时为空</param>
/// <param name="EditPermission">修改它需要的权限点</param>
/// <param name="AllowedActions">当前用户可执行的动作</param>
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>
/// 一个系统设置：定义、默认值、当前值，以及当前用户能对它做什么
/// </summary>
/// <param name="Key">设置键</param>
/// <param name="DisplayName">显示名</param>
/// <param name="Module">声明它的模块</param>
/// <param name="Group">分组</param>
/// <param name="Fields">值里的成员清单，管理页据此生成编辑控件</param>
/// <param name="DefaultValue">代码里的默认值</param>
/// <param name="CurrentValue">当前生效的值：被修改过就是修改后的，否则是默认值</param>
/// <param name="IsModified">是否被修改过</param>
/// <param name="ConcurrencyVersion">值行的版本号，输出为 version；还在用默认值时为空</param>
/// <param name="EditPermission">修改它需要的权限点</param>
/// <param name="AllowedActions">当前用户现在可以对它执行的动作</param>
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

using System.Text.Json;

namespace Kmic.Configuration.Presentation.Preferences;

/// <summary>
/// 设置偏好在系统层或角色层的值
/// </summary>
/// <param name="Value">偏好值，须为 JSON 对象；已声明偏好按值类型解析并校验，偏好族仅保存 JSON 内容</param>
/// <param name="ExpectedVersion">预期版本号，当前层级尚无值时不传；不匹配时返回 409 Common.VersionConflict</param>
public sealed record SetPreferenceLayerRequest(JsonElement Value, long? ExpectedVersion);

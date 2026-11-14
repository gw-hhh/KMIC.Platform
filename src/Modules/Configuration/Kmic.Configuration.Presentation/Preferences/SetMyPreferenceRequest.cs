using System.Text.Json;

namespace Kmic.Configuration.Presentation.Preferences;

/// <summary>
/// 保存本人偏好的请求
/// </summary>
/// <param name="Value">偏好值，须为 JSON 对象；已声明偏好按值类型解析并校验，偏好族仅保存 JSON 内容</param>
public sealed record SetMyPreferenceRequest(JsonElement Value);

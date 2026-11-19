using System.Text.Json;

namespace Kmic.Configuration.Presentation.Preferences;

/// <summary>
/// 设置偏好在系统层或角色层的值
/// </summary>
/// <param name="Value">偏好值</param>
/// <param name="ExpectedVersion">预期版本号，还没有值时不传</param>
public sealed record SetPreferenceLayerRequest(JsonElement Value, long? ExpectedVersion);

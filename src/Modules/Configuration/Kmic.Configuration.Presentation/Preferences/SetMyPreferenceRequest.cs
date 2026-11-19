using System.Text.Json;

namespace Kmic.Configuration.Presentation.Preferences;

/// <summary>
/// 保存本人偏好的请求
/// </summary>
/// <param name="Value">偏好值</param>
public sealed record SetMyPreferenceRequest(JsonElement Value);

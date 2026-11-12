using System.Text.Json;

namespace Kmic.Configuration.Presentation.Preferences;

/// <summary>
/// 保存本人偏好的请求
/// </summary>
/// <param name="Value">值，JSON 对象；目录里声明的偏好按它的成员读出并校验，偏好族里的不理解内容</param>
public sealed record SetMyPreferenceRequest(JsonElement Value);

using System.Text.Json;

namespace Kmic.Configuration.Presentation.Preferences;

/// <summary>
/// 设置偏好在系统层或角色层的值
/// </summary>
/// <param name="Value">值，JSON 对象；目录里声明的偏好按它的成员读出并校验，偏好族里的不理解内容</param>
/// <param name="ExpectedVersion">调用方看到的版本号；这一层还没有值时不给。别人改过就返回 409 Common.VersionConflict</param>
public sealed record SetPreferenceLayerRequest(JsonElement Value, long? ExpectedVersion);

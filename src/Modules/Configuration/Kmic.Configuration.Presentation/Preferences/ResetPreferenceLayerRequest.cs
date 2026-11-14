namespace Kmic.Configuration.Presentation.Preferences;

/// <summary>
/// 删除偏好在系统层或角色层的值
/// </summary>
/// <param name="ExpectedVersion">预期版本号，记录已变更或删除时返回 409 Common.VersionConflict</param>
public sealed record ResetPreferenceLayerRequest(long ExpectedVersion);

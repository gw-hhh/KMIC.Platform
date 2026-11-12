namespace Kmic.Configuration.Presentation.Preferences;

/// <summary>
/// 删除偏好在系统层或角色层的值
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过或已经删除就返回 409 Common.VersionConflict</param>
public sealed record ResetPreferenceLayerRequest(long ExpectedVersion);

namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 启用角色的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record EnableRoleRequest(long ExpectedVersion);

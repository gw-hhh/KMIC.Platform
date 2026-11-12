namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 收回角色的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的用户版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record RevokeRoleRequest(long ExpectedVersion);

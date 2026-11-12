namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 停用角色的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="Reason">原因，记入状态历史</param>
public sealed record DisableRoleRequest(long ExpectedVersion, string? Reason);

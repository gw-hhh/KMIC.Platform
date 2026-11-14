namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 停用角色的请求
/// </summary>
/// <param name="ExpectedVersion">预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="Reason">原因，记入状态历史</param>
public sealed record DisableRoleRequest(long ExpectedVersion, string? Reason);

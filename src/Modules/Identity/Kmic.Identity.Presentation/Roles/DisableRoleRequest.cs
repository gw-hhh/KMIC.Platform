namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 停用角色的请求
/// </summary>
/// <param name="ExpectedVersion">预期版本号</param>
/// <param name="Reason">原因</param>
public sealed record DisableRoleRequest(long ExpectedVersion, string? Reason);

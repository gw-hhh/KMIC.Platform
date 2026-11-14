namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 启用角色的请求
/// </summary>
/// <param name="ExpectedVersion">预期版本号，不匹配时返回 409 Common.VersionConflict</param>
public sealed record EnableRoleRequest(long ExpectedVersion);

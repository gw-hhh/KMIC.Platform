namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 启用角色的请求
/// </summary>
/// <param name="ExpectedVersion">预期版本号</param>
public sealed record EnableRoleRequest(long ExpectedVersion);

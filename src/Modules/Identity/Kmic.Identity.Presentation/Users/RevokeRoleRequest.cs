namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 收回角色的请求
/// </summary>
/// <param name="ExpectedVersion">用户的预期版本号</param>
public sealed record RevokeRoleRequest(long ExpectedVersion);

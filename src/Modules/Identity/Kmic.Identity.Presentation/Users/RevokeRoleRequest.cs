namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 收回角色的请求
/// </summary>
/// <param name="ExpectedVersion">用户的预期版本号，不匹配时返回 409 Common.VersionConflict</param>
public sealed record RevokeRoleRequest(long ExpectedVersion);

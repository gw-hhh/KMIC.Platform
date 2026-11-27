namespace Kmic.Identity.Presentation.Users;

/// <param name="ExpectedVersion">用户的预期版本号</param>
public sealed record RevokeRoleRequest(long ExpectedVersion);

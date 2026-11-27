namespace Kmic.Identity.Presentation.Roles;

/// <param name="ExpectedVersion">预期版本号</param>
/// <param name="Permissions">新的权限点集合</param>
public sealed record SetRolePermissionsRequest(long ExpectedVersion, IReadOnlyList<string> Permissions);

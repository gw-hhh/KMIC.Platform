namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 替换角色权限点的请求
/// </summary>
/// <param name="ExpectedVersion">预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="Permissions">新的权限点集合，只接受目录里存在的编码</param>
public sealed record SetRolePermissionsRequest(long ExpectedVersion, IReadOnlyList<string> Permissions);

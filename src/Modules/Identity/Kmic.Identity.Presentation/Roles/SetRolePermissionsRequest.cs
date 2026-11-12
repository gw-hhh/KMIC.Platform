namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 替换角色权限点的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="Permissions">新的权限点集合，只接受目录里存在的编码</param>
public sealed record SetRolePermissionsRequest(long ExpectedVersion, IReadOnlyList<string> Permissions);

namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 修改角色的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="Name">名称</param>
/// <param name="Description">说明</param>
/// <param name="SortOrder">次序</param>
public sealed record UpdateRoleRequest(long ExpectedVersion, string Name, string? Description, int SortOrder);

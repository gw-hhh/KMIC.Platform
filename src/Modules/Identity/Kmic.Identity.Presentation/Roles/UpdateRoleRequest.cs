namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 修改角色的请求
/// </summary>
/// <param name="ExpectedVersion">预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="Name">名称</param>
/// <param name="Description">说明</param>
/// <param name="SortOrder">次序</param>
public sealed record UpdateRoleRequest(long ExpectedVersion, string Name, string? Description, int SortOrder);

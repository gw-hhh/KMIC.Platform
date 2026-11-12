namespace Kmic.Identity.Application.Roles.Get;

/// <summary>
/// 角色详情里的一个权限点
/// </summary>
/// <param name="Code">权限点编码</param>
/// <param name="DisplayName">显示名；已退役的权限点目录里没有，为空</param>
/// <param name="Retired">是否已退役：代码里已经没有这个权限点，替换权限点时会去掉</param>
public sealed record RolePermissionItem(string Code, string? DisplayName, bool Retired);

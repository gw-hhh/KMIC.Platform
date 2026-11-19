namespace Kmic.Identity.Application.Roles.Get;

/// <summary>
/// 角色拥有的权限点
/// </summary>
/// <param name="Code">权限点编码</param>
/// <param name="DisplayName">显示名称，代码目录中已不存在的权限点为空</param>
/// <param name="Retired">是否已从代码目录移除</param>
public sealed record RolePermissionItem(string Code, string? DisplayName, bool Retired);

using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 新建角色的请求
/// </summary>
/// <param name="Name">名称</param>
/// <param name="Description">说明</param>
/// <param name="SortOrder">登录时角色下拉里的次序</param>
/// <param name="DefaultScopeLevel">默认数据范围等级：Self、OrgUnit、OrgUnitAndBelow、All</param>
public sealed record CreateRoleRequest(string Name, string? Description, int SortOrder, RoleScopeLevel DefaultScopeLevel);

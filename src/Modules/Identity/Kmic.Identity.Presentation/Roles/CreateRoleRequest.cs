using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Presentation.Roles;

/// <param name="Name">名称</param>
/// <param name="Description">说明</param>
/// <param name="SortOrder">角色排序，用于登录时的角色选择列表</param>
/// <param name="DefaultScopeLevel">默认数据范围等级：Self、OrgUnit、OrgUnitAndBelow、All</param>
public sealed record CreateRoleRequest(string Name, string? Description, int SortOrder, RoleScopeLevel DefaultScopeLevel);

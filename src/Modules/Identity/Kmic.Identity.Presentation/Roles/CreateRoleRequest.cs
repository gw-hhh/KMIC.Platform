using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 新建角色的请求
/// </summary>
/// <param name="Name">名称，最多 100 个字符；不得重复（不区分大小写）</param>
/// <param name="Description">说明，最多 500 个字符</param>
/// <param name="SortOrder">登录时角色下拉里的次序，0 到 10000</param>
/// <param name="DefaultScopeLevel">默认数据范围等级：Self、OrgUnit、OrgUnitAndBelow、All</param>
public sealed record CreateRoleRequest(string Name, string? Description, int SortOrder, RoleScopeLevel DefaultScopeLevel);

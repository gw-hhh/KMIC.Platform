using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.List;

/// <param name="Q">关键字</param>
/// <param name="Status">状态编码</param>
/// <param name="RoleId">筛选角色，仅返回拥有该角色的用户</param>
/// <param name="OrgUnitId">组织单元 ID，仅筛选直属该单元的用户</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(IdentityPermissions.UserView)]
public sealed record ListUsersQuery(string? Q, IReadOnlyList<string>? Status, RoleId? RoleId, OrgUnitId? OrgUnitId, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<UserListItem>>;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.List;

/// <summary>
/// 查询用户列表
/// </summary>
/// <param name="Q">关键字，在账号、姓名、英文名、工号、手机号里模糊匹配</param>
/// <param name="Status">状态编码，可以给多个</param>
/// <param name="RoleId">只看拥有这个角色的用户</param>
/// <param name="OrgUnitId">只看直接属于这个组织单元的用户</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(IdentityPermissions.UserView)]
public sealed record ListUsersQuery(string? Q, IReadOnlyList<string>? Status, RoleId? RoleId, OrgUnitId? OrgUnitId, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<UserListItem>>;

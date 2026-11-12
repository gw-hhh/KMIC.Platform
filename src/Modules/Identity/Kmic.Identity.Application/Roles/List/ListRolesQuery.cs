using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Roles.List;

/// <summary>
/// 查询角色列表
/// </summary>
/// <param name="Q">关键字，在名称和说明里模糊匹配</param>
/// <param name="Status">状态编码，可以给多个</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(IdentityPermissions.RoleView)]
public sealed record ListRolesQuery(string? Q, IReadOnlyList<string>? Status, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<RoleListItem>>;

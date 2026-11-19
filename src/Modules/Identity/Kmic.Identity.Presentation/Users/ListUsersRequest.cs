using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 查询用户列表的参数
/// </summary>
/// <param name="Q">关键字</param>
/// <param name="Status">状态编码</param>
/// <param name="RoleId">只看拥有这个角色的用户</param>
/// <param name="OrgUnitId">只看直接属于这个组织单元的用户</param>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
/// <param name="Sort">排序</param>
public sealed record ListUsersRequest(string? Q, string[]? Status, RoleId? RoleId, OrgUnitId? OrgUnitId, int Page = 1, int PageSize = 20, string? Sort = null);

using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 查询用户列表的参数
/// </summary>
/// <param name="Q">关键字，在账号、姓名、英文名、工号、手机号里模糊匹配</param>
/// <param name="Status">状态编码，重复参数名可以给多个，如 status=Active&amp;status=Disabled</param>
/// <param name="RoleId">只看拥有这个角色的用户</param>
/// <param name="OrgUnitId">只看直接属于这个组织单元的用户</param>
/// <param name="Page">页码，从 1 开始</param>
/// <param name="PageSize">每页数量，1 到 200，默认 20</param>
/// <param name="Sort">排序，如 displayName,-account：逗号分隔，字段名前加 - 表示倒序，最多 3 个字段</param>
public sealed record ListUsersRequest(string? Q, string[]? Status, RoleId? RoleId, OrgUnitId? OrgUnitId, int Page = 1, int PageSize = 20, string? Sort = null);

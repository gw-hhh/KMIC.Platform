namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 查询角色列表的参数
/// </summary>
/// <param name="Q">关键字</param>
/// <param name="Status">状态编码</param>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
/// <param name="Sort">排序</param>
public sealed record ListRolesRequest(string? Q, string[]? Status, int Page = 1, int PageSize = 20, string? Sort = null);

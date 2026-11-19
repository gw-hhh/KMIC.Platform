namespace Kmic.Identity.Presentation.SecurityEvents;

/// <summary>
/// 查询安全事件的参数
/// </summary>
/// <param name="From">起始时间</param>
/// <param name="To">结束时间</param>
/// <param name="Type">按类型筛选</param>
/// <param name="UserId">按用户筛选</param>
/// <param name="ClientIp">按客户端 IP 筛选</param>
/// <param name="Account">按事件关联的账号筛选</param>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
/// <param name="Sort">排序</param>
public sealed record ListSecurityEventsRequest(DateTimeOffset From, DateTimeOffset To, string? Type, Guid? UserId, string? ClientIp, string? Account, int Page = 1, int PageSize = 20, string? Sort = null);

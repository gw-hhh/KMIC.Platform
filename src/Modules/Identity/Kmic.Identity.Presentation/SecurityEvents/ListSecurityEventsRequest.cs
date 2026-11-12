namespace Kmic.Identity.Presentation.SecurityEvents;

/// <summary>
/// 查询安全事件的参数
/// </summary>
/// <param name="From">起始时间，含；必填</param>
/// <param name="To">结束时间，不含；必填，与起始时间相隔最长 31 天</param>
/// <param name="Type">按类型筛选，如 LoginFailed、AuthorizationDenied、AuthorizationConfigChanged</param>
/// <param name="UserId">按用户筛选</param>
/// <param name="ClientIp">按客户端 IP 筛选</param>
/// <param name="Account">按登录失败时输入的账号筛选</param>
/// <param name="Page">页码，从 1 开始</param>
/// <param name="PageSize">每页数量，1 到 200，默认 20</param>
/// <param name="Sort">排序，如 -occurredAt,type：逗号分隔，字段名前加 - 表示倒序，最多 3 个字段</param>
public sealed record ListSecurityEventsRequest(DateTimeOffset From, DateTimeOffset To, string? Type, Guid? UserId, string? ClientIp, string? Account, int Page = 1, int PageSize = 20, string? Sort = null);

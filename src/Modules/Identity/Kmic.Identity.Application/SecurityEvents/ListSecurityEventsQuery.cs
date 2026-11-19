using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.SecurityEvents;

/// <summary>
/// 查询登录失败、越权访问、授权变更和模拟身份等安全事件
/// </summary>
/// <remarks>
/// 时间范围必填，最长 31 天
/// </remarks>
/// <param name="From">起始时间，含</param>
/// <param name="To">结束时间，不含</param>
/// <param name="Type">按类型筛选</param>
/// <param name="UserId">按用户筛选</param>
/// <param name="ClientIp">按客户端 IP 筛选</param>
/// <param name="Account">按事件关联的账号筛选</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(IdentityPermissions.SecurityEventView)]
[AuditedRead]
public sealed record ListSecurityEventsQuery(DateTimeOffset From, DateTimeOffset To, string? Type, Guid? UserId, string? ClientIp, string? Account, PageRequest Page, SortRequest Sort)
    : IQuery<PagedResult<SecurityEventItem>>;

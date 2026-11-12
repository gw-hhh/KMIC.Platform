using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.SecurityEvents;

/// <summary>
/// 查询安全事件：登录失败、越权访问、授权配置变更、模拟身份这些
/// </summary>
/// <remarks>
/// 时间范围必填，最长 31 天：这张表只追加、数据量大，不允许无范围地翻。查询本身是敏感操作，会记读审计
/// </remarks>
/// <param name="From">起始时间，含</param>
/// <param name="To">结束时间，不含</param>
/// <param name="Type">按类型筛选</param>
/// <param name="UserId">按用户筛选</param>
/// <param name="ClientIp">按客户端 IP 筛选</param>
/// <param name="Account">按登录失败时输入的账号筛选</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(IdentityPermissions.SecurityEventView)]
[AuditedRead]
public sealed record ListSecurityEventsQuery(DateTimeOffset From, DateTimeOffset To, string? Type, Guid? UserId, string? ClientIp, string? Account, PageRequest Page, SortRequest Sort)
    : IQuery<PagedResult<SecurityEventItem>>;

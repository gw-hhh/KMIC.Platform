using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Captured.List;

/// <summary>
/// 查询捕获的通知内容列表
/// </summary>
/// <remarks>
/// 列表不含正文；未开启捕获时返回 NotFound。内容可能含验证码，查询须记录读审计
/// </remarks>
/// <param name="Channel">按渠道筛选：Email、Sms</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(NotificationsPermissions.CapturedView)]
[AuditedRead]
public sealed record ListCapturedQuery(string? Channel, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<CapturedListItem>>;

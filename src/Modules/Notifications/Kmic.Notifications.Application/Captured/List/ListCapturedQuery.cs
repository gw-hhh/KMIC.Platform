using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Captured.List;

/// <summary>查询捕获内容列表（未开启捕获时返回 NotFound）</summary>
/// <param name="Channel">按渠道筛选：Email、Sms</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(NotificationsPermissions.CapturedView)]
[AuditedRead]
public sealed record ListCapturedQuery(string? Channel, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<CapturedListItem>>;

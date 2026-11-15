using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Deliveries.List;

/// <summary>
/// 查询投递记录
/// </summary>
/// <remarks>
/// 列表不含正文，收件地址已脱敏；查询成功后记录读审计
/// </remarks>
/// <param name="RecipientUserId">按收件用户筛选</param>
/// <param name="Channel">按渠道筛选：Email、Sms</param>
/// <param name="Status">按状态筛选：Pending、Sending、Sent、Failed、Skipped、Captured</param>
/// <param name="TemplateKey">按模板键筛选</param>
/// <param name="From">创建时间下限（含）</param>
/// <param name="To">创建时间上限（不含）</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(NotificationsPermissions.DeliveryView)]
[AuditedRead]
public sealed record ListDeliveriesQuery(
    Guid? RecipientUserId,
    string? Channel,
    string? Status,
    string? TemplateKey,
    DateTimeOffset? From,
    DateTimeOffset? To,
    PageRequest Page,
    SortRequest Sort) : IQuery<PagedResult<DeliveryListItem>>;

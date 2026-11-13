using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Captured.List;

/// <summary>
/// 查询非生产环境捕获的通知内容：本该发出去的邮件和短信
/// </summary>
/// <remarks>
/// 不显示正文。没有开启捕获（生产环境）时返回找不到。内容可能含验证码，查询会记读审计
/// </remarks>
/// <param name="Channel">按渠道筛选：Email、Sms</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(NotificationsPermissions.CapturedView)]
[AuditedRead]
public sealed record ListCapturedQuery(string? Channel, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<CapturedListItem>>;

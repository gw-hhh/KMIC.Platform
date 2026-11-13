using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Deliveries.Get;

/// <summary>
/// 查询一条投递：包含渲染后的主题和正文
/// </summary>
/// <remarks>
/// 含密模板的投递发完或最终失败后，正文已经清空，显示为空。查看正文会记读审计
/// </remarks>
/// <param name="Id">投递</param>
[RequirePermission(NotificationsPermissions.DeliveryView)]
[AuditedRead]
public sealed record GetDeliveryQuery(DeliveryId Id) : IQuery<DeliveryDetailResponse>;

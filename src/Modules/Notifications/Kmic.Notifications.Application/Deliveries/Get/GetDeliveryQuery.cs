using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Deliveries.Get;

/// <summary>查询投递详情</summary>
[RequirePermission(NotificationsPermissions.DeliveryView)]
[AuditedRead]
public sealed record GetDeliveryQuery(DeliveryId Id) : IQuery<DeliveryDetailResponse>;

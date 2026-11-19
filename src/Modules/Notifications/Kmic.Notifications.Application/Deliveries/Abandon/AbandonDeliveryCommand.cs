using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Deliveries.Abandon;

/// <summary>
/// 放弃失败的投递
/// </summary>
[RequirePermission(NotificationsPermissions.DeliveryManage)]
[Audited]
[Idempotent]
public sealed record AbandonDeliveryCommand(DeliveryId Id, string Reason) : ICommand;

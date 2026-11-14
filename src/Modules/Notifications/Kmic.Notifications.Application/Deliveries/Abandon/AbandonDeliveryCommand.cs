using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Deliveries.Abandon;

/// <summary>
/// 放弃失败的投递
/// </summary>
/// <remarks>
/// 须填写放弃原因；放弃记录在保留期满后可由清理任务删除
/// </remarks>
[RequirePermission(NotificationsPermissions.DeliveryManage)]
[Audited]
[Idempotent]
public sealed record AbandonDeliveryCommand(DeliveryId Id, string Reason) : ICommand;

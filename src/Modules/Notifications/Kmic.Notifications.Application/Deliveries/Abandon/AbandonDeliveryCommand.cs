using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Deliveries.Abandon;

/// <summary>
/// 放弃一条失败的投递
/// </summary>
/// <remarks>
/// 状态仍是失败，加上放弃标记，之后不再出现在待处理清单里，过了保留期由清理任务删除。只有失败、而且还没放弃的投递可以放弃
/// </remarks>
/// <param name="Id">投递</param>
/// <param name="Reason">放弃的原因，必填</param>
[RequirePermission(NotificationsPermissions.DeliveryManage)]
[Audited]
[Idempotent]
public sealed record AbandonDeliveryCommand(DeliveryId Id, string Reason) : ICommand;

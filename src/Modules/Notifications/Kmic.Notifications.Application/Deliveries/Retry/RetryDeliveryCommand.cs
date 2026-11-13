using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Deliveries.Retry;

/// <summary>
/// 重试一条失败的投递
/// </summary>
/// <remarks>
/// 放回队列，尝试次数清零。只有失败、而且没有放弃的投递可以重试；含密通知超过有效期的不能重试
/// </remarks>
/// <param name="Id">投递</param>
[RequirePermission(NotificationsPermissions.DeliveryManage)]
[Audited]
[Idempotent]
public sealed record RetryDeliveryCommand(DeliveryId Id) : ICommand;

using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.Notifications;

/// <summary>
/// 发送请求记录的仓储
/// </summary>
public interface INotificationRepository : IRepository<Notification, NotificationId>
{
    /// <summary>
    /// 这条来源消息是不是已经处理过
    /// </summary>
    /// <param name="sourceMessageId">来源消息的 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<bool> ExistsForAsync(Guid sourceMessageId, CancellationToken ct);
}

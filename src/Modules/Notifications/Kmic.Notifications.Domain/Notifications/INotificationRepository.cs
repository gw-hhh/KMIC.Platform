using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.Notifications;

/// <summary>
/// 发送请求记录的仓储
/// </summary>
public interface INotificationRepository : IRepository<Notification, NotificationId>
{
    /// <summary>
    /// 指定来源消息是否已处理
    /// </summary>
    Task<bool> ExistsForAsync(Guid sourceMessageId, CancellationToken ct);

    /// <summary>
    /// 删除已无投递记录且创建时间早于 <paramref name="createdBefore" /> 的发送请求
    /// </summary>
    /// <returns>删除条数</returns>
    Task<int> PurgeAsync(DateTimeOffset createdBefore, CancellationToken ct);
}

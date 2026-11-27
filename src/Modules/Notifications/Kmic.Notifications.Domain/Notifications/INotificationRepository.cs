using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.Notifications;

/// <summary>发送请求记录仓储</summary>
public interface INotificationRepository : IRepository<Notification, NotificationId>
{
    /// <summary>指定来源消息是否已处理</summary>
    Task<bool> ExistsForAsync(Guid sourceMessageId, CancellationToken ct);

    /// <summary>删除已无关联投递且创建时间早于 <paramref name="createdBefore" /> 的发送请求，返回删除条数</summary>
    Task<int> PurgeAsync(DateTimeOffset createdBefore, CancellationToken ct);
}

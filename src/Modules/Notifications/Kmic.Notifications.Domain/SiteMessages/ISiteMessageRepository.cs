using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.SiteMessages;

/// <summary>站内消息仓储</summary>
public interface ISiteMessageRepository : IRepository<SiteMessage, SiteMessageId>
{
    /// <summary>将指定用户的全部未读消息标为已读，返回更新条数</summary>
    Task<int> MarkAllReadAsync(UserId recipient, DateTimeOffset now, CancellationToken ct);

    /// <summary>按已读和未读各自的截止时间清理站内消息，返回删除条数</summary>
    Task<int> PurgeAsync(DateTimeOffset readBefore, DateTimeOffset unreadCreatedBefore, CancellationToken ct);
}

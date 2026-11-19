using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.SiteMessages;

/// <summary>
/// 站内消息的仓储
/// </summary>
public interface ISiteMessageRepository : IRepository<SiteMessage, SiteMessageId>
{
    /// <summary>
    /// 将用户的全部未读消息标为已读
    /// </summary>
    /// <returns>标为已读的消息数</returns>
    Task<int> MarkAllReadAsync(UserId recipient, DateTimeOffset now, CancellationToken ct);

    /// <summary>
    /// 按已读和未读各自的截止时间删除站内消息
    /// </summary>
    /// <returns>删除的消息数</returns>
    Task<int> PurgeAsync(DateTimeOffset readBefore, DateTimeOffset unreadCreatedBefore, CancellationToken ct);
}

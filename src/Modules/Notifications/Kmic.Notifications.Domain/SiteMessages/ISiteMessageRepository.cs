using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.SiteMessages;

/// <summary>
/// 站内消息的仓储
/// </summary>
public interface ISiteMessageRepository : IRepository<SiteMessage, SiteMessageId>
{
    /// <summary>
    /// 把一个用户的全部未读消息标为已读
    /// </summary>
    /// <param name="recipient">收件人</param>
    /// <param name="now">已读时间</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>标了几条</returns>
    Task<int> MarkAllReadAsync(UserId recipient, DateTimeOffset now, CancellationToken ct);

    /// <summary>
    /// 删除过期的消息：已读的超过一个期限，未读的超过另一个期限
    /// </summary>
    /// <param name="readBefore">已读时间早于它的删除</param>
    /// <param name="unreadCreatedBefore">没读过且创建时间早于它的删除</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>删了几条</returns>
    Task<int> PurgeAsync(DateTimeOffset readBefore, DateTimeOffset unreadCreatedBefore, CancellationToken ct);
}

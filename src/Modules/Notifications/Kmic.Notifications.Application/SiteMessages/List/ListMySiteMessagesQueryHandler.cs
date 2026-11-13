using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Application.SiteMessages.List;

/// <summary>
/// 处理 <see cref="ListMySiteMessagesQuery"/>
/// </summary>
public sealed class ListMySiteMessagesQueryHandler(INotificationsQueryDb db, IExecutionContext execution)
    : IQueryHandler<ListMySiteMessagesQuery, PagedResult<SiteMessageItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<PagedResult<SiteMessageItem>>> Handle(ListMySiteMessagesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var me = (execution.Access ?? throw new InvalidOperationException("查询我的站内消息需要已登录的访问上下文。")).UserId;
        var messages = db.SiteMessages.Where(m => m.RecipientUserId == me);
        if (query.Unread is { } unread)
        {
            messages = unread ? messages.Where(m => m.ReadAt == null) : messages.Where(m => m.ReadAt != null);
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            messages = messages.Where(m => m.Category == query.Category);
        }

        return await messages
            .Select(m => new SiteMessageItem(m.Id, m.Title, m.Body, m.Link, m.Category, m.SentAt, m.ReadAt != null, m.ReadAt))
            .ToPagedAsync(query.Page, query.Sort, SiteMessageSort.Map, cancellationToken);
    }
}

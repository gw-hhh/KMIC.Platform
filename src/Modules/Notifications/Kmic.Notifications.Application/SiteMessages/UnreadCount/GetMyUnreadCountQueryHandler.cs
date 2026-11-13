using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Application.SiteMessages.UnreadCount;

/// <summary>
/// 处理 <see cref="GetMyUnreadCountQuery"/>
/// </summary>
public sealed class GetMyUnreadCountQueryHandler(INotificationsQueryDb db, IExecutionContext execution)
    : IQueryHandler<GetMyUnreadCountQuery, UnreadCountResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<UnreadCountResponse>> Handle(GetMyUnreadCountQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var me = (execution.Access ?? throw new InvalidOperationException("查询未读数需要已登录的访问上下文。")).UserId;
        var count = await db.SiteMessages.CountAsync(m => m.RecipientUserId == me && m.ReadAt == null, cancellationToken);
        return new UnreadCountResponse(count);
    }
}

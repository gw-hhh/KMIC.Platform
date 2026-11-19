using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.SiteMessages;

namespace Kmic.Notifications.Application.SiteMessages.MarkRead;

/// <summary>
/// 将本人的全部未读站内消息标为已读
/// </summary>
public sealed class MarkAllSiteMessagesReadCommandHandler(ISiteMessageRepository messages, IExecutionContext execution, TimeProvider time)
    : ICommandHandler<MarkAllSiteMessagesReadCommand, MarkedReadResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<MarkedReadResponse>> Handle(MarkAllSiteMessagesReadCommand command, CancellationToken cancellationToken)
    {
        var me = (execution.Access ?? throw new InvalidOperationException("标记已读需要已登录的访问上下文。")).UserId;
        return new MarkedReadResponse(await messages.MarkAllReadAsync(me, time.GetUtcNow(), cancellationToken));
    }
}

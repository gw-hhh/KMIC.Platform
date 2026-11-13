using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.SiteMessages;

namespace Kmic.Notifications.Application.SiteMessages.MarkRead;

/// <summary>
/// 处理 <see cref="MarkSiteMessageReadCommand"/>
/// </summary>
public sealed class MarkSiteMessageReadCommandHandler(ISiteMessageRepository messages, IExecutionContext execution, TimeProvider time)
    : ICommandHandler<MarkSiteMessageReadCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(MarkSiteMessageReadCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var me = (execution.Access ?? throw new InvalidOperationException("标记已读需要已登录的访问上下文。")).UserId;
        var loaded = await messages.GetForUpdateAsync(command.Id, null, cancellationToken);
        if (loaded.IsFailure || loaded.Value.RecipientUserId != me)
        {
            return NotificationsErrors.SiteMessageNotFound;
        }

        loaded.Value.MarkRead(time.GetUtcNow());
        return Result.Success();
    }
}

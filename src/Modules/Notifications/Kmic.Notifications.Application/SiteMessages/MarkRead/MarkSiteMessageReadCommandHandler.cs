using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.SiteMessages;

namespace Kmic.Notifications.Application.SiteMessages.MarkRead;

public sealed class MarkSiteMessageReadCommandHandler(ISiteMessageRepository messages, IExecutionContext execution, TimeProvider time)
    : ICommandHandler<MarkSiteMessageReadCommand>
{
    public async ValueTask<Result> Handle(MarkSiteMessageReadCommand command, CancellationToken cancellationToken)
    {
        var me = (execution.Access ?? throw new InvalidOperationException("标记已读需要已登录的用户身份。")).UserId;
        var loaded = await messages.GetForUpdateAsync(command.Id, null, cancellationToken);
        if (loaded.IsFailure || loaded.Value.RecipientUserId != me)
        {
            return NotificationsErrors.SiteMessageNotFound;
        }

        loaded.Value.MarkRead(time.GetUtcNow());
        return Result.Success();
    }
}

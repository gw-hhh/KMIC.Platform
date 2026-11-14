using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.SiteMessages;
using Microsoft.Extensions.Options;

namespace Kmic.Notifications.Application.SiteMessages.Purge;

/// <summary>
/// 按保留期清理站内消息
/// </summary>
public sealed class PurgeSiteMessagesCommandHandler(ISiteMessageRepository messages, IOptions<NotificationsRetentionOptions> options, TimeProvider time)
    : ICommandHandler<PurgeSiteMessagesCommand, ScheduledJobOutcome>
{
    /// <inheritdoc />
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(PurgeSiteMessagesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = time.GetUtcNow();
        var purged = await messages.PurgeAsync(now.AddDays(-options.Value.SiteMessageReadDays), now.AddDays(-options.Value.SiteMessageUnreadDays), cancellationToken);
        return new ScheduledJobOutcome(purged);
    }
}

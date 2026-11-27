using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.SiteMessages;
using Microsoft.Extensions.Options;

namespace Kmic.Notifications.Application.SiteMessages.Purge;

public sealed class PurgeSiteMessagesCommandHandler(ISiteMessageRepository messages, IOptions<NotificationsRetentionOptions> options, TimeProvider time)
    : ICommandHandler<PurgeSiteMessagesCommand, ScheduledJobOutcome>
{
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(PurgeSiteMessagesCommand command, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        var purged = await messages.PurgeAsync(now.AddDays(-options.Value.SiteMessageReadDays), now.AddDays(-options.Value.SiteMessageUnreadDays), cancellationToken);
        return new ScheduledJobOutcome(purged);
    }
}

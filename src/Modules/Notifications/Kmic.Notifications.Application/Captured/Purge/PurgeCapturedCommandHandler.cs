using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Captured;
using Microsoft.Extensions.Options;

namespace Kmic.Notifications.Application.Captured.Purge;

/// <summary>
/// 处理 <see cref="PurgeCapturedCommand"/>
/// </summary>
public sealed class PurgeCapturedCommandHandler(ICapturedMessageRepository captured, IOptions<NotificationsRetentionOptions> options, TimeProvider time)
    : ICommandHandler<PurgeCapturedCommand, ScheduledJobOutcome>
{
    /// <inheritdoc />
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(PurgeCapturedCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var purged = await captured.PurgeAsync(time.GetUtcNow().AddDays(-options.Value.CapturedDays), cancellationToken);
        return new ScheduledJobOutcome(purged);
    }
}

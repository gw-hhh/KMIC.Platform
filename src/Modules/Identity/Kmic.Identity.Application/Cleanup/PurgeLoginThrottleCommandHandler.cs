using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authentication;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Cleanup;

public sealed class PurgeLoginThrottleCommandHandler(ILoginThrottleStore throttles, IOptions<LoginThrottleOptions> options, TimeProvider time)
    : ICommandHandler<PurgeLoginThrottleCommand, ScheduledJobOutcome>
{
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(PurgeLoginThrottleCommand command, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        var purged = await throttles.PurgeAsync(now - options.Value.ResetAfter, now, cancellationToken);
        return new ScheduledJobOutcome(purged);
    }
}

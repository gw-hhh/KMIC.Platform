using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Domain.Sessions;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Cleanup;

public sealed class PurgeSessionsCommandHandler(ISessionRepository sessions, IOptions<SessionLifetimeOptions> options, TimeProvider time)
    : ICommandHandler<PurgeSessionsCommand, ScheduledJobOutcome>
{
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(PurgeSessionsCommand command, CancellationToken cancellationToken)
    {
        var purged = await sessions.PurgeEndedAsync(time.GetUtcNow() - options.Value.EndedRetention, cancellationToken);
        return new ScheduledJobOutcome(purged);
    }
}

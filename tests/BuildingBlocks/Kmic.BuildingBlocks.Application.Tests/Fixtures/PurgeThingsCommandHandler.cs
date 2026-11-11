using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

public sealed class PurgeThingsCommandHandler : ICommandHandler<PurgeThingsCommand, ScheduledJobOutcome>
{
    public ValueTask<Result<ScheduledJobOutcome>> Handle(PurgeThingsCommand command, CancellationToken cancellationToken) =>
        ValueTask.FromResult<Result<ScheduledJobOutcome>>(new ScheduledJobOutcome(0));
}

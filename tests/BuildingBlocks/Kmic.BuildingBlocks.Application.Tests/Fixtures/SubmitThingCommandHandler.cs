using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

public sealed class SubmitThingCommandHandler(HandlerProbe probe, IExecutionContext context) : ICommandHandler<SubmitThingCommand, Guid>
{
    public ValueTask<Result<Guid>> Handle(SubmitThingCommand command, CancellationToken cancellationToken)
    {
        probe.Record(nameof(SubmitThingCommandHandler), context.Origin);
        return ValueTask.FromResult<Result<Guid>>(Guid.CreateVersion7());
    }
}

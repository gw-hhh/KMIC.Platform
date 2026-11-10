using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

public sealed class SyncThingCommandHandler(HandlerProbe probe, IExecutionContext context) : ICommandHandler<SyncThingCommand>
{
    public ValueTask<Result> Handle(SyncThingCommand command, CancellationToken cancellationToken)
    {
        probe.Record(nameof(SyncThingCommandHandler), context.Origin);
        return ValueTask.FromResult(Result.Success());
    }
}

using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

public sealed class GatedThingQueryHandler(HandlerProbe probe, IExecutionContext execution) : IQueryHandler<GatedThingQuery, string>
{
    public ValueTask<Result<string>> Handle(GatedThingQuery query, CancellationToken cancellationToken)
    {
        probe.Record(nameof(GatedThingQueryHandler), execution.Origin);
        return ValueTask.FromResult<Result<string>>("开放");
    }
}

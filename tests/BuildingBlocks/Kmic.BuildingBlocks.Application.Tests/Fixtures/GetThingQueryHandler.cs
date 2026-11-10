using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

public sealed class GetThingQueryHandler(HandlerProbe probe, IExecutionContext context) : IQueryHandler<GetThingQuery, string>
{
    public ValueTask<Result<string>> Handle(GetThingQuery query, CancellationToken cancellationToken)
    {
        probe.Record(nameof(GetThingQueryHandler), context.Origin);
        return ValueTask.FromResult<Result<string>>("东西");
    }
}

using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

public sealed class LookupThingQueryHandler(HandlerProbe probe, IExecutionContext context) : IQueryHandler<LookupThingQuery, string>
{
    public ValueTask<Result<string>> Handle(LookupThingQuery query, CancellationToken cancellationToken)
    {
        probe.Record(nameof(LookupThingQueryHandler), context.Origin);
        return ValueTask.FromResult<Result<string>>("引用");
    }
}

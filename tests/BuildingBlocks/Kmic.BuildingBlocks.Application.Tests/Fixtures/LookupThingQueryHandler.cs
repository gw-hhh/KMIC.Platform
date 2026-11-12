using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

public sealed class LookupThingQueryHandler(HandlerProbe probe, IExecutionContext context) : IQueryHandler<LookupThingQuery, ReferenceInfo>
{
    public ValueTask<Result<ReferenceInfo>> Handle(LookupThingQuery query, CancellationToken cancellationToken)
    {
        probe.Record(nameof(LookupThingQueryHandler), context.Origin);
        return ValueTask.FromResult<Result<ReferenceInfo>>(new ReferenceInfo(Guid.Empty, "引用", null, ReferenceStatus.Active));
    }
}

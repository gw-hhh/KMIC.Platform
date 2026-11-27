using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.Extensions.Time.Testing;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

// 推进假时钟以模拟耗时，避免真实等待
public sealed class SlowThingQueryHandler(TimeProvider time) : IQueryHandler<SlowThingQuery, int>
{
    public ValueTask<Result<int>> Handle(SlowThingQuery query, CancellationToken cancellationToken)
    {
        ((FakeTimeProvider)time).Advance(TimeSpan.FromSeconds(2));
        return ValueTask.FromResult<Result<int>>(1);
    }
}

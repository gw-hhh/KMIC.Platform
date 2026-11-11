using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence.InvalidModel;

/// <summary>
/// 映射到其他 schema，且没有归属却声明了未登记的范围对象
/// </summary>
internal sealed class Widget : AggregateRoot<TicketId>
{
    private Widget(TicketId id)
        : base(id)
    {
    }
}

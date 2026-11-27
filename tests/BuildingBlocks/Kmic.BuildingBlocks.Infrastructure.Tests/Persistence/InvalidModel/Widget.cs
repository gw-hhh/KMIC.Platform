using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence.InvalidModel;

// 故意映射到其他 schema，且无归属却声明了未定义的范围对象
internal sealed class Widget : AggregateRoot<TicketId>
{
    private Widget(TicketId id)
        : base(id)
    {
    }
}

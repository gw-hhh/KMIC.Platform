using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence.InvalidModel;

// 故意引用其他模块的范围对象
internal sealed class Sprocket : AggregateRoot<TicketId>, IHasOrgUnit
{
    private Sprocket(TicketId id)
        : base(id)
    {
    }

    public OrgUnitId OrgUnitId { get; private set; }
}

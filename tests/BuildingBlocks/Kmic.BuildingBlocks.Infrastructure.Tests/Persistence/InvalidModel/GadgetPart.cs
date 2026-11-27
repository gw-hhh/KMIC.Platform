using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence.InvalidModel;

// 故意违规的子实体：实现了软删除，且保留指向父实体的导航
internal sealed class GadgetPart : Entity<TicketLineId>, ISoftDeletable
{
    private GadgetPart(TicketLineId id)
        : base(id)
    {
    }

    public Gadget? Gadget { get; private set; }
}

using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence.InvalidModel;

/// <summary>
/// 故意写错的子实体：实现了软删除，还保留了指回父实体的导航
/// </summary>
internal sealed class GadgetPart : Entity<TicketLineId>, ISoftDeletable
{
    private GadgetPart(TicketLineId id)
        : base(id)
    {
    }

    public Gadget? Gadget { get; private set; }
}

using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence.InvalidModel;

/// <summary>
/// 子实体：错误地实现了软删除，并保留了指回父实体的导航
/// </summary>
internal sealed class GadgetPart : Entity<TicketLineId>, ISoftDeletable
{
    private GadgetPart(TicketLineId id)
        : base(id)
    {
    }

    public Gadget? Gadget { get; private set; }
}

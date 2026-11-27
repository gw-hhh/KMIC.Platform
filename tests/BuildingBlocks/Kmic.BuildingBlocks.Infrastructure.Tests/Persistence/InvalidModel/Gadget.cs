using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence.InvalidModel;

// 故意违反映射规则的聚合根，供 ModelValidationTests 使用
internal sealed class Gadget : AggregateRoot<TicketId>, IHasOwner
{
    private readonly List<GadgetPart> _parts = [];

    private Gadget(TicketId id)
        : base(id)
    {
    }

    public UserId OwnerId { get; private set; }

    public decimal Price { get; private set; }

    public double Ratio { get; private set; }

    public DateTime MadeAt { get; private set; }

    public string Name { get; private set; } = "";

    public string PasswordHash { get; private set; } = "";

    public Widget? Widget { get; private set; }

    public IReadOnlyList<GadgetPart> Parts => _parts;
}

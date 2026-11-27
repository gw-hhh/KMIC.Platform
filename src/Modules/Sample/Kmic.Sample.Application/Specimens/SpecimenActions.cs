using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens;

public sealed class SpecimenActions : ResourceActionCatalog<Specimen, SpecimenAction>
{
    protected override void Define(IResourceActionBuilder<Specimen, SpecimenAction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Action(SpecimenAction.Edit, SamplePermissions.Edit, state: s => s.CanEditAttachments());
        builder.Action(SpecimenAction.Submit, SamplePermissions.Submit, state: s => s.CanFire(SpecimenAction.Submit));
        builder.Action(SpecimenAction.Close, SamplePermissions.Close, state: s => s.CanFire(SpecimenAction.Close));
    }
}

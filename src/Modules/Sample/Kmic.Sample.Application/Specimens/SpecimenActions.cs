using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens;

/// <summary>
/// 示例标本的资源动作目录：详情响应里的“可用动作”据此计算
/// </summary>
public sealed class SpecimenActions : ResourceActionCatalog<Specimen, SpecimenAction>
{
    /// <inheritdoc />
    protected override void Define(IResourceActionBuilder<Specimen, SpecimenAction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Action(SpecimenAction.Edit, SamplePermissions.Edit, state: s => s.CanEditAttachments());
        builder.Action(SpecimenAction.Submit, SamplePermissions.Submit, state: s => s.CanFire(SpecimenAction.Submit));
        builder.Action(SpecimenAction.Close, SamplePermissions.Close, state: s => s.CanFire(SpecimenAction.Close));
    }
}

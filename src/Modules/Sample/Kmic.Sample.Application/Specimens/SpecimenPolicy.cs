using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens;

/// <summary>
/// 示例的操作权限
/// </summary>
public sealed class SpecimenPolicy : IResourcePolicy<Specimen, SpecimenAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, Specimen resource, SpecimenAction action)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);

        return resource.OwnerId == access.UserId ? PolicyDecision.Allow : PolicyDecision.Deny(DenialReason.ResourceDenied, SampleErrors.NotOwner);
    }
}

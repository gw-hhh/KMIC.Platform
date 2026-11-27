using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens;

/// <summary>仅允许示例负责人执行操作</summary>
public sealed class SpecimenPolicy : IResourcePolicy<Specimen, SpecimenAction>
{
    public PolicyDecision Evaluate(IAccessContext access, Specimen resource, SpecimenAction action)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);

        return resource.OwnerId == access.UserId ? PolicyDecision.Allow : PolicyDecision.Deny(DenialReason.ResourceDenied, SampleErrors.NotOwner);
    }
}

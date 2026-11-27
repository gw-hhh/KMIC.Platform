using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Operations.Application.ScheduledJobs;

public sealed class ScheduledJobPolicy : IResourcePolicy<ScheduledJobTarget, ScheduledJobAction>
{
    public PolicyDecision Evaluate(IAccessContext access, ScheduledJobTarget resource, ScheduledJobAction action) => PolicyDecision.Allow;
}

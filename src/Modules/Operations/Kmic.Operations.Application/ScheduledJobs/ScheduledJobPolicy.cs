using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 定时任务资源策略
/// </summary>
public sealed class ScheduledJobPolicy : IResourcePolicy<ScheduledJobTarget, ScheduledJobAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, ScheduledJobTarget resource, ScheduledJobAction action) => PolicyDecision.Allow;
}

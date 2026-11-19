using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 定时任务资源策略
/// </summary>
/// <remarks>
/// 不按任务追加限制，权限由用例检查
/// </remarks>
public sealed class ScheduledJobPolicy : IResourcePolicy<ScheduledJobTarget, ScheduledJobAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, ScheduledJobTarget resource, ScheduledJobAction action) => PolicyDecision.Allow;
}

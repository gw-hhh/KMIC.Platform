using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 定时任务的资源策略：没有按资源的限制，权限点就是授权的全部
/// </summary>
/// <remarks>
/// 动作目录要求每类资源有一个策略，所以这里放一条全部允许的。危险操作的保护靠确认对话框、
/// 状态条件（任务正在运行时拒绝立即执行）和审计，不靠“谁拥有这个任务”
/// </remarks>
public sealed class ScheduledJobPolicy : IResourcePolicy<ScheduledJobTarget, ScheduledJobAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, ScheduledJobTarget resource, ScheduledJobAction action) => PolicyDecision.Allow;
}

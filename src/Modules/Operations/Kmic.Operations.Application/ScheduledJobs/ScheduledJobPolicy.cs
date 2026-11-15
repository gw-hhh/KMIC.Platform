using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 定时任务资源策略
/// </summary>
/// <remarks>
/// 访问权限由用例或动作目录检查，此策略不按任务归属追加限制。
/// 运行状态检查及审计由具体操作负责
/// </remarks>
public sealed class ScheduledJobPolicy : IResourcePolicy<ScheduledJobTarget, ScheduledJobAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, ScheduledJobTarget resource, ScheduledJobAction action) => PolicyDecision.Allow;
}

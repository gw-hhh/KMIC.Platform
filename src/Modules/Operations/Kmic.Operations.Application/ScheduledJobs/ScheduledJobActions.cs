using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 定时任务的资源动作目录
/// </summary>
public sealed class ScheduledJobActions : ResourceActionCatalog<ScheduledJobTarget, ScheduledJobAction>
{
    /// <inheritdoc />
    protected override void Define(IResourceActionBuilder<ScheduledJobTarget, ScheduledJobAction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Action(ScheduledJobAction.Edit, OperationsPermissions.JobsManage, state: IsDeclared);
        builder.Action(ScheduledJobAction.RestoreDefault, OperationsPermissions.JobsManage, state: HasOverride);
        builder.Action(ScheduledJobAction.RunNow, OperationsPermissions.JobsManage, state: IsDeclared);
    }

    private static Result IsDeclared(ScheduledJobTarget target) => target.IsDeclared ? Result.Success() : OperationsErrors.JobNotFound;

    private static Result HasOverride(ScheduledJobTarget target) => target.IsDeclared && target.HasOverride ? Result.Success() : OperationsErrors.JobNotFound;
}

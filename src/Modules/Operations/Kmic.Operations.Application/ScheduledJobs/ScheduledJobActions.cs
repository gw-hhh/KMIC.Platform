using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.ScheduledJobs;

public sealed class ScheduledJobActions : ResourceActionCatalog<ScheduledJobTarget, ScheduledJobAction>
{
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

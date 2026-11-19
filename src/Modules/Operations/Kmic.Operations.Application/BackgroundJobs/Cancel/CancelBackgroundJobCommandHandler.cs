using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.BackgroundJobs.Cancel;

/// <summary>
/// 请求取消后台作业
/// </summary>
/// <remarks>
/// 不在 Operations 的命令事务里
/// </remarks>
public sealed class CancelBackgroundJobCommandHandler(IModuleJobStores stores, IExecutionContext execution) : ICommandHandler<CancelBackgroundJobCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(CancelBackgroundJobCommand command, CancellationToken cancellationToken)
    {
        var store = stores.Find(command.Module);
        if (store is null || await store.FindAsync(command.JobId, cancellationToken) is null)
        {
            return OperationsErrors.BackgroundJobNotFound;
        }

        var user = execution.User;
        var by = user.ActualUserId ?? user.UserId ?? UserId.System;
        return await store.RequestCancelAsync(command.JobId, by, cancellationToken) ? Result.Success() : OperationsErrors.BackgroundJobFinished;
    }
}

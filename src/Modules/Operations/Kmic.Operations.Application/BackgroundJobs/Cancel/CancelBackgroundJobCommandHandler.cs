using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.BackgroundJobs.Cancel;

// 直接操作目标模块作业存储，不纳入 Operations 本地事务
public sealed class CancelBackgroundJobCommandHandler(IModuleJobStores stores, IExecutionContext execution) : ICommandHandler<CancelBackgroundJobCommand>
{
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

using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

/// <summary>
/// 请求取消本人提交的后台作业
/// </summary>
/// <remarks>
/// 不在 Operations 的命令事务里
/// </remarks>
public sealed class CancelMyJobCommandHandler(IModuleJobStores stores, IExecutionContext execution) : ICommandHandler<CancelMyJobCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(CancelMyJobCommand command, CancellationToken cancellationToken)
    {
        var job = await MyJobs.FindAsync(execution, stores, command.Module, command.JobId, cancellationToken);
        if (job is null)
        {
            return OperationsErrors.BackgroundJobNotFound;
        }

        var user = execution.User;
        var by = user.ActualUserId ?? user.UserId ?? UserId.System;
        var store = stores.Find(command.Module)!;
        return await store.RequestCancelAsync(command.JobId, by, cancellationToken) ? Result.Success() : OperationsErrors.BackgroundJobFinished;
    }
}

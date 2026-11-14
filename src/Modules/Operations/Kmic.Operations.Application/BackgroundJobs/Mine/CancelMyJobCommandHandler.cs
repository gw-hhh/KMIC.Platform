using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

/// <summary>
/// 取消本人提交的后台作业
/// </summary>
/// <remarks>
/// 直接更新作业所属模块的存储，不参与 Operations 命令事务
/// </remarks>
public sealed class CancelMyJobCommandHandler(IModuleJobStores stores, IExecutionContext execution) : ICommandHandler<CancelMyJobCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(CancelMyJobCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

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

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
/// 取消请求直接更新作业所属模块的存储，不参与 Operations 命令事务
/// </remarks>
public sealed class CancelBackgroundJobCommandHandler(IModuleJobStores stores, IExecutionContext execution) : ICommandHandler<CancelBackgroundJobCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(CancelBackgroundJobCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

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

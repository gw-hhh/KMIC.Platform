using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.BackgroundJobs.Cancel;

/// <summary>
/// 处理 <see cref="CancelBackgroundJobCommand"/>
/// </summary>
/// <remarks>
/// 取消写的是作业所在模块的表，不在 Operations 命令的事务里：这是框架存储的基础设施写入，和执行器自己改作业状态一样
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

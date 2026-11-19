using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.ScheduledJobs.RunNow;

/// <summary>
/// 立即触发定时任务
/// </summary>
/// <remarks>
/// 触发请求不在命令事务里，事务失败撤不回已提交的触发，所以任务要能重复执行
/// </remarks>
public sealed class RunJobNowCommandHandler(IScheduledJobControl control, IExecutionContext execution) : ICommandHandler<RunJobNowCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(RunJobNowCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 模拟登录时记录实际操作人
        var user = execution.User;
        var by = user.ActualUserId ?? user.UserId ?? UserId.System;
        return await control.TriggerAsync(command.JobName, by, cancellationToken) switch
        {
            ScheduledJobTriggerOutcome.Triggered => Result.Success(),
            ScheduledJobTriggerOutcome.AlreadyRunning => OperationsErrors.JobAlreadyRunning,
            _ => OperationsErrors.JobNotFound,
        };
    }
}

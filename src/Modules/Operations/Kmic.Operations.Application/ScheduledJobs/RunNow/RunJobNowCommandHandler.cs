using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.ScheduledJobs.RunNow;

/// <summary>
/// 处理 <see cref="RunJobNowCommand"/>
/// </summary>
/// <remarks>
/// 触发是调度库里的写入，不在命令的事务里：事务之后才失败时，任务已经交给调度器了。这一点接受，
/// 任务本来就要求可以重复执行
/// </remarks>
public sealed class RunJobNowCommandHandler(IScheduledJobControl control, IExecutionContext execution) : ICommandHandler<RunJobNowCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(RunJobNowCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 模拟登录时记真正动手的人
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

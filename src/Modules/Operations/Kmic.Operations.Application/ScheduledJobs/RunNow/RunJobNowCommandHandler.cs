using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.ScheduledJobs.RunNow;

// 触发操作不纳入命令事务，任务本身须具备幂等性
public sealed class RunJobNowCommandHandler(IScheduledJobControl control, IExecutionContext execution) : ICommandHandler<RunJobNowCommand>
{
    public async ValueTask<Result> Handle(RunJobNowCommand command, CancellationToken cancellationToken)
    {
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

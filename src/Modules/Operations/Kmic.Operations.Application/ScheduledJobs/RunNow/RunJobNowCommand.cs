using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.RunNow;

/// <summary>手动触发一次定时任务；任务正在执行或已排队时拒绝触发</summary>
/// <param name="JobName">任务名</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
[ExemptFromMessagingSuspension]
public sealed record RunJobNowCommand(string JobName) : ICommand;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.RunNow;

/// <summary>
/// 手动触发一次定时任务
/// </summary>
/// <remarks>
/// 任务忙或已在排队时拒绝
/// </remarks>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
public sealed record RunJobNowCommand(string JobName) : ICommand;

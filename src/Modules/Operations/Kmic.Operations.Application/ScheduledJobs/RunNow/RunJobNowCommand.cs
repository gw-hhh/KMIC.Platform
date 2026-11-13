using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.RunNow;

/// <summary>
/// 立即执行一次定时任务
/// </summary>
/// <remarks>
/// 交给运行着调度器的实例执行，执行记录里的触发方式是“手动”、触发人是操作人。
/// 任务正在运行时拒绝，不排队。停用的任务也可以手动执行
/// </remarks>
/// <param name="JobName">任务名</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
public sealed record RunJobNowCommand(string JobName) : ICommand;

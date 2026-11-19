using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.RunNow;

/// <summary>
/// 手动触发一次定时任务
/// </summary>
/// <remarks>
/// 提交触发请求后即返回，不等待任务完成；执行记录标记为“手动”，并记录操作人。
/// 任务正在执行、即将按计划执行或已有手动触发在排队时拒绝。停用的任务也可以手动执行
/// </remarks>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
public sealed record RunJobNowCommand(string JobName) : ICommand;

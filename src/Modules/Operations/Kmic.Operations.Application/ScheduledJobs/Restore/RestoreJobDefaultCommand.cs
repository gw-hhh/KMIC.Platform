using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs.Restore;

/// <summary>
/// 恢复定时任务的默认设置
/// </summary>
/// <remarks>
/// 清空自定义启用状态和执行计划，保留设置记录；之后使用代码默认值
/// </remarks>
/// <param name="JobName">任务名</param>
/// <param name="ExpectedVersion">调用方看到的版本号</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
public sealed record RestoreJobDefaultCommand(string JobName, long ExpectedVersion) : ICommand<VersionedResponse<ScheduledJobSettingId>>;

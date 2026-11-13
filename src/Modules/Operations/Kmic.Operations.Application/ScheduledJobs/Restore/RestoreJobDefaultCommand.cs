using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs.Restore;

/// <summary>
/// 把一个定时任务恢复成代码里的默认开关与执行时间
/// </summary>
/// <remarks>
/// 设置行保留、两项清空，便于追溯；之后跟着代码里的默认值变化
/// </remarks>
/// <param name="JobName">任务名</param>
/// <param name="ExpectedVersion">调用方看到的版本号</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
public sealed record RestoreJobDefaultCommand(string JobName, long ExpectedVersion) : ICommand<VersionedResponse<ScheduledJobSettingId>>;

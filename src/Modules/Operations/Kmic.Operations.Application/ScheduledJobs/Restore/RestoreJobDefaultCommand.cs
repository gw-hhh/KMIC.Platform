using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs.Restore;

/// <summary>
/// 恢复定时任务的默认设置
/// </summary>
/// <param name="JobName">任务名</param>
/// <param name="ExpectedVersion">调用方看到的版本号</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
[ExemptFromMessagingSuspension]
public sealed record RestoreJobDefaultCommand(string JobName, long ExpectedVersion) : ICommand<VersionedResponse<ScheduledJobSettingId>>;

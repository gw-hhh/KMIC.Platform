using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs.Set;

/// <summary>
/// 设置定时任务开关和执行时间
/// </summary>
/// <remarks>
/// 同步调度器失败不回滚已保存的设置
/// </remarks>
/// <param name="JobName">任务名</param>
/// <param name="Enabled">是否启用</param>
/// <param name="Cron">Cron 表达式</param>
/// <param name="ExpectedVersion">自定义设置的预期版本号</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
[ExemptFromMessagingSuspension]
public sealed record SetJobOverrideCommand(string JobName, bool? Enabled, string? Cron, long? ExpectedVersion) : ICommand<VersionedResponse<ScheduledJobSettingId>>;

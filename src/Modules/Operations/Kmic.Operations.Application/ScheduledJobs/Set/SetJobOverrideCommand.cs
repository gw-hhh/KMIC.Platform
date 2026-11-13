using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs.Set;

/// <summary>
/// 改一个定时任务的开关和执行时间
/// </summary>
/// <remarks>
/// 整体替换覆盖：给空的项恢复成跟着代码里的默认值。保存后同步到调度器，整个集群即时生效；
/// 同步失败不影响保存，列表里会显示“还没生效”
/// </remarks>
/// <param name="JobName">任务名</param>
/// <param name="Enabled">是否启用；空表示跟着默认值（启用）</param>
/// <param name="Cron">执行时间的 Cron 表达式；空表示跟着代码里的默认值</param>
/// <param name="ExpectedVersion">调用方看到的版本号；这个任务还没改过时不给。别人改过就返回版本冲突</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
public sealed record SetJobOverrideCommand(string JobName, bool? Enabled, string? Cron, long? ExpectedVersion) : ICommand<VersionedResponse<ScheduledJobSettingId>>;

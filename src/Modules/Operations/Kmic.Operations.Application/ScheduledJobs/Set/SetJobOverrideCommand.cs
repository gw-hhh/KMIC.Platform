using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs.Set;

/// <summary>
/// 设置定时任务开关和执行时间
/// </summary>
/// <remarks>
/// 事务提交后尝试同步调度器，同步失败不回滚已保存的设置
/// </remarks>
/// <param name="JobName">任务名</param>
/// <param name="Enabled">是否启用；空表示跟着默认值（启用）</param>
/// <param name="Cron">执行时间的 Cron 表达式；空表示跟着代码里的默认值</param>
/// <param name="ExpectedVersion">自定义设置的预期版本号，尚无设置时不传；不匹配时返回版本冲突</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
public sealed record SetJobOverrideCommand(string JobName, bool? Enabled, string? Cron, long? ExpectedVersion) : ICommand<VersionedResponse<ScheduledJobSettingId>>;

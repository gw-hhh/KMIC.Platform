using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Catalog;

/// <summary>清理已废弃且超过保留期的权限点、范围对象及其角色配置</summary>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeRetiredPermissionsCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

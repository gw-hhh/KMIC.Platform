using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 清理超过保留期的退役权限、数据范围对象及角色配置
/// </summary>
/// <remarks>
/// 退役后保留角色配置以支持版本回滚，保留期满后才删除
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeRetiredPermissionsCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

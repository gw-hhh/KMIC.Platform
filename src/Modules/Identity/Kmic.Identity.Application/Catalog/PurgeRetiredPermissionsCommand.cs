using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 清理退役很久的权限点与范围对象，以及角色上对它们的配置
/// </summary>
/// <remarks>
/// 每周执行。退役是为了版本回滚时角色配置不丢；退役超过保留期之后回滚的可能性可以忽略，再留着只会让表和角色越来越脏
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeRetiredPermissionsCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

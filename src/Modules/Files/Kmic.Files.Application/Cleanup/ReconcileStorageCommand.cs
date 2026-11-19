using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 对账：核对存储里的对象和库里的文件记录
/// </summary>
/// <remarks>
/// 差异超过上限时整轮不动
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("状态变更记入文件历史，对账结果记入作业运行记录")]
public sealed record ReconcileStorageCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

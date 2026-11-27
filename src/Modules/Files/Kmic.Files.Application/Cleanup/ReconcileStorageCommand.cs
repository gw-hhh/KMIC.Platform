using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Cleanup;

/// <summary>核对对象存储与数据库文件记录的一致性，差异超限时中止本轮处理</summary>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("状态变更记入文件历史，对账结果记入作业运行记录")]
public sealed record ReconcileStorageCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

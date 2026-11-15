using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 对账：核对存储里的对象和库里的文件记录
/// </summary>
/// <remarks>
/// 文件按登记时间、对象按文件 ID 中的创建时间判断是否达到 FilesOptions 配置的等待期；无法取得创建时间的对象不删除。
/// 无记录且无未完成登记请求的对象会删除；缺失对象记为存储问题，可还原时保留文件状态，否则标为被拒绝。
/// 有待核对文件但存储清单为空，或任一类差异数超过上限时，本次不删除对象或修改文件
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("状态变更记入文件历史，对账结果记入作业运行记录")]
public sealed record ReconcileStorageCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

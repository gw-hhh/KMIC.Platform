using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 对账：核对存储里的对象和库里的文件记录
/// </summary>
/// <remarks>
/// 仅核对创建已满指定时长的对象和文件；时长及差异上限由 FilesOptions 配置。
/// 无记录且无未完成登记请求的对象会删除；缺失对象记为存储问题，可还原时保留文件状态，否则标为被拒绝。
/// 存储列表异常为空或任一类差异超过上限时，停止本次处理，不删除对象或修改文件
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时对账，文件的状态变化记入文件状态历史，结果记入作业运行记录")]
public sealed record ReconcileStorageCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

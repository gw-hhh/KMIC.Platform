using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 对账：核对存储里的对象和库里的文件记录
/// </summary>
/// <remarks>
/// 每周日执行，两个方向。存储里有、库里没有的对象（上传中途崩溃，或生成的文件一直没登记）直接删除，
/// 删除前探查有没有未完成的登记请求，有就跳过。库里有记录、存储里找不到对象的文件记为存储问题：
/// 记录的版本还在、只是被删除标记盖住的，等运维还原，文件不动；版本也不在的，文件按被拒绝处理。
/// 只看存在了至少两天的对象和文件。差异多得不正常时整个放弃，什么也不改
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时对账，文件的状态变化记入文件状态历史，结果记入作业运行记录")]
public sealed record ReconcileStorageCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 把到期的文件标记为“删除中”：临时文件、解除关联后保留期满的文件、被拒绝的文件
/// </summary>
/// <remarks>
/// 每小时执行，只做标记，对象和行由删除工作器在事务之外删除。临时文件和已解除关联的文件按关联与清理的互斥协议处理：
/// 对每个候选取排他锁（取不到的是业务正在关联它，这次跳过）、探查有没有未完成的关联请求（有的跳过）、再改状态。
/// 每次最多处理一批，剩下的下一轮再处理
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，文件的状态变化记入文件状态历史，结果记入作业运行记录")]
public sealed record PurgeExpiredFilesCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

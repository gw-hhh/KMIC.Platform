using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 将到期的临时文件、已解除关联文件和被拒绝文件标记为“删除中”
/// </summary>
/// <remarks>
/// 每次处理一批；存储对象及数据库记录由删除工作器另行删除。
/// 临时文件和已解除关联文件须先取得排他锁并探查未完成的关联请求；取锁失败或存在请求时跳过，锁持有至事务结束
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("状态变更记入文件历史，清理结果记入作业运行记录")]
public sealed record PurgeExpiredFilesCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

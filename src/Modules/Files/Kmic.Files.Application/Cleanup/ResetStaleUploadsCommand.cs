using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 将超时未完成的上传标记为被拒绝
/// </summary>
/// <remarks>
/// 处理客户端断开或服务重启等导致的上传中断。被拒绝的文件保留期满后可进入删除队列；未登记的存储对象由对账任务处理
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("状态变更记入文件历史，清理结果记入作业运行记录")]
public sealed record ResetStaleUploadsCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

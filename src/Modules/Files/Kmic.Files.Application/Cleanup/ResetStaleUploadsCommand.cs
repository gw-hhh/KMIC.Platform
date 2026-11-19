using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 将超时未完成的上传标记为被拒绝
/// </summary>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("状态变更记入文件历史，清理结果记入作业运行记录")]
public sealed record ResetStaleUploadsCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 把上传中断的文件标记为被拒绝：开始上传之后一直没有结果的，客户端断开或服务重启都会这样
/// </summary>
/// <remarks>
/// 每小时执行。被拒绝的文件保留一小段时间之后由到期清理标记为“删除中”；已经写进存储的部分一并清理，
/// 没来得及登记的对象由对账发现
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，文件的状态变化记入文件状态历史，结果记入作业运行记录")]
public sealed record ResetStaleUploadsCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Deliveries.Purge;

/// <summary>
/// 清空含密通知投递里还留着的正文和短信参数
/// </summary>
/// <remarks>
/// 兜底：正常情况下投递发完或最终失败时就已经清空，这里补救进程崩溃等极端情况。每小时执行一次。
/// 非生产环境的捕获内容不在这里处理，捕获的验证码按设计留给测试人员看
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record ClearSecretBodiesCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;

using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 声明本模块的定时任务：按时间发送本模块的 [SystemOnly] 命令
/// </summary>
public interface ISchedulingBuilder
{
    /// <summary>
    /// 声明一个定时任务
    /// </summary>
    void Schedule<TCommand>(string name, string cron, Func<ScheduledRun, TCommand> create, Action<ScheduleOptions>? configure = null)
        where TCommand : ICommand<ScheduledJobOutcome>;
}

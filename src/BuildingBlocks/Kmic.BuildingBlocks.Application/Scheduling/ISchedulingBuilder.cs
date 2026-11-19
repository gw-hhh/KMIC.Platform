using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 声明本模块的定时任务
/// </summary>
/// <remarks>
/// 一个定时任务就是按时间发送一个本模块的 [SystemOnly] 命令。模块只用这个接口，不引用 Quartz
/// </remarks>
public interface ISchedulingBuilder
{
    /// <summary>
    /// 声明一个定时任务
    /// </summary>
    void Schedule<TCommand>(string name, string cron, Func<ScheduledRun, TCommand> create, Action<ScheduleOptions>? configure = null)
        where TCommand : ICommand<ScheduledJobOutcome>;
}

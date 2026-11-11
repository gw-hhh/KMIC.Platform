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
    /// <param name="name">稳定名称，以本模块名开头，如 "Sample.SendRepairReminders"</param>
    /// <param name="cron">默认执行时间，Quartz 的 Cron 写法（带秒），按业务时区解释</param>
    /// <param name="create">按这次触发构造命令。同一个计划时刻执行两次，结果要相同</param>
    /// <param name="configure">错过触发时怎么办、显示名</param>
    void Schedule<TCommand>(string name, string cron, Func<ScheduledRun, TCommand> create, Action<ScheduleOptions>? configure = null)
        where TCommand : ICommand<ScheduledJobOutcome>;
}

using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>模块定时任务构建器，按计划发送本模块的 [SystemOnly] 命令。</summary>
public interface ISchedulingBuilder
{
    void Schedule<TCommand>(string name, string cron, Func<ScheduledRun, TCommand> create, Action<ScheduleOptions>? configure = null)
        where TCommand : ICommand<ScheduledJobOutcome>;
}

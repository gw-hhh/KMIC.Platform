using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 一个模块声明的定时任务，校验放在定时任务目录里统一做
/// </summary>
internal sealed class ModuleSchedules(string module) : ISchedulingBuilder
{
    private readonly List<DeclaredSchedule> _schedules = [];

    public string Module { get; } = module;

    public IReadOnlyList<DeclaredSchedule> Schedules => _schedules;

    public void Schedule<TCommand>(string name, string cron, Func<ScheduledRun, TCommand> create, Action<ScheduleOptions>? configure = null)
        where TCommand : ICommand<ScheduledJobOutcome>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(cron);
        ArgumentNullException.ThrowIfNull(create);

        var options = new ScheduleOptions();
        configure?.Invoke(options);
        _schedules.Add(new DeclaredSchedule(name, typeof(TCommand), cron, run => create(run), options));
    }
}

using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 模块写下的一条定时任务声明，还没校验
/// </summary>
internal sealed record DeclaredSchedule(
    string Name,
    Type CommandType,
    string Cron,
    Func<ScheduledRun, ICommand<ScheduledJobOutcome>> Create,
    ScheduleOptions Options);

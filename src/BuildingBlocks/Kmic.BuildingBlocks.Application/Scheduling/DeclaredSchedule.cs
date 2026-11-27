using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Scheduling;

internal sealed record DeclaredSchedule(
    string Name,
    Type CommandType,
    string Cron,
    Func<ScheduledRun, ICommand<ScheduledJobOutcome>> Create,
    ScheduleOptions Options);

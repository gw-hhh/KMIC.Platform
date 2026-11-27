using Kmic.BuildingBlocks.Application.Scheduling;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

internal sealed record ScheduleEntry(string Name, string Module, string Cron, Misfire Misfire, string DisplayName);

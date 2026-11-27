using Kmic.BuildingBlocks.Application.Scheduling;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

internal interface IFrameworkTask
{
    // 须以 "Framework." 开头
    string Name { get; }

    // 按业务时区解释
    string Cron { get; }

    string DisplayName { get; }

    Task<ScheduledJobOutcome> ExecuteAsync(CancellationToken ct);
}

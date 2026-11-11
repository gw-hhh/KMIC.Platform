using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Maintenance;

/// <summary>
/// 每小时删除各模块已过期的幂等记录
/// </summary>
internal sealed class PurgeIdempotencyTask(ModuleFrameworkTables tables, DataSources dataSources, IOptions<PurgeOptions> options) : IFrameworkTask
{
    public string Name => "Framework.PurgeIdempotency";

    public string Cron => "0 0 * * * ?";

    public string DisplayName => "清理过期的幂等记录";

    public async Task<ScheduledJobOutcome> ExecuteAsync(CancellationToken ct)
    {
        long total = 0;
        foreach (var module in tables.Modules)
        {
            total += await BatchDelete.RunAsync(
                dataSources.Transactions(CallOrigin.ScheduledJob),
                tables.For(module).Idempotency,
                """t."ExpiresAt" < clock_timestamp()""",
                new Dictionary<string, object>(StringComparer.Ordinal),
                options.Value.BatchSize,
                ct);
        }

        return new ScheduledJobOutcome(checked((int)total));
    }
}

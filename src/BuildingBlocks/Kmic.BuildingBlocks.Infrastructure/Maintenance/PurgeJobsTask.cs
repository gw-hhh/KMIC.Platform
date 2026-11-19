using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Maintenance;

/// <summary>
/// 每天删除过了保留期的后台作业记录
/// </summary>
/// <remarks>
/// 只删作业行；结果文件由 Files 按登记时的保留期清理
/// </remarks>
internal sealed class PurgeJobsTask(ModuleFrameworkTables tables, DataSources dataSources, IOptions<PurgeOptions> options) : IFrameworkTask
{
    public string Name => "Framework.PurgeJobs";

    public string Cron => "0 10 3 * * ?";

    public string DisplayName => "清理过期的后台作业记录";

    public async Task<ScheduledJobOutcome> ExecuteAsync(CancellationToken ct)
    {
        long total = 0;
        foreach (var module in tables.Modules)
        {
            total += await BatchDelete.RunAsync(
                dataSources.Transactions(CallOrigin.ScheduledJob),
                tables.For(module).Jobs,
                """t."ExpiresAt" < clock_timestamp()""",
                new Dictionary<string, object>(StringComparer.Ordinal),
                options.Value.BatchSize,
                ct);
        }

        return new ScheduledJobOutcome(checked((int)total));
    }
}

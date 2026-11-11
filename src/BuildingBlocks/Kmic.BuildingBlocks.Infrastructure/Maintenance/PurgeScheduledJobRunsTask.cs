using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Maintenance;

/// <summary>
/// 每天把停在“执行中”太久的执行记录标为中断，删除过了保留期的记录
/// </summary>
internal sealed class PurgeScheduledJobRunsTask(DataSources dataSources, IOptions<PurgeOptions> options, ILogger<PurgeScheduledJobRunsTask> logger) : IFrameworkTask
{
    public string Name => "Framework.PurgeScheduledJobRuns";

    public string Cron => "0 20 3 * * ?";

    public string DisplayName => "清理定时任务的执行记录";

    public async Task<ScheduledJobOutcome> ExecuteAsync(CancellationToken ct)
    {
        var database = dataSources.Transactions(CallOrigin.ScheduledJob);
        int abandoned;
        await using (var command = database.CreateCommand(
            $"""
            UPDATE {SchedulingTables.Runs}
            SET "Status" = {ScheduledRunStatus.Abandoned}, "FinishedAt" = clock_timestamp(), "ErrorMessage" = '执行中实例退出，结果未知'
            WHERE "Status" = {ScheduledRunStatus.Running} AND "StartedAt" < clock_timestamp() - @after
            """))
        {
            command.Parameters.AddWithValue("after", options.Value.AbandonRunningAfter);
            abandoned = await command.ExecuteNonQueryAsync(ct);
        }

        if (abandoned > 0)
        {
            SchedulingLog.RunsAbandoned(logger, abandoned);
        }

        var deleted = await BatchDelete.RunAsync(
            database,
            SchedulingTables.Runs,
            """t."FinishedAt" < clock_timestamp() - @retention""",
            new Dictionary<string, object>(StringComparer.Ordinal) { ["retention"] = options.Value.ScheduledRunRetention },
            options.Value.BatchSize,
            ct);
        return new ScheduledJobOutcome(checked((int)deleted), $"删除 {deleted} 条，标为中断 {abandoned} 条");
    }
}

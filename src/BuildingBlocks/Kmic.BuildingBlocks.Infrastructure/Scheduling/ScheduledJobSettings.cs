using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 读定时任务的覆盖值
/// </summary>
/// <remarks>
/// 写入由 Operations 模块的管理命令负责，框架只读
/// </remarks>
internal sealed class ScheduledJobSettings(DataSources dataSources)
{
    public async Task<IReadOnlyDictionary<string, ScheduledJobSetting>> ReadAsync(CancellationToken ct)
    {
        await using var command = dataSources.Transactions(CallOrigin.Startup).CreateCommand(
            $"""SELECT "JobName", "Enabled", "Cron" FROM {SchedulingTables.Settings}""");
        await using var reader = await command.ExecuteReaderAsync(ct);
        var settings = new Dictionary<string, ScheduledJobSetting>(StringComparer.Ordinal);
        while (await reader.ReadAsync(ct))
        {
            settings[reader.GetString(0)] = new ScheduledJobSetting(
                await reader.IsDBNullAsync(1, ct) ? null : reader.GetBoolean(1),
                await reader.IsDBNullAsync(2, ct) ? null : reader.GetString(2));
        }

        return settings;
    }
}

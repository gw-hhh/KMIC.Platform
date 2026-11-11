using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 写执行记录
/// </summary>
/// <remarks>
/// 开始和结束各用一个独立的短事务，不放进命令的事务：命令回滚了，失败的记录也要留下
/// </remarks>
internal sealed class ScheduledJobRuns(DataSources dataSources)
{
    private NpgsqlDataSource Database => dataSources.Transactions(CallOrigin.ScheduledJob);

    public async Task<Guid> StartAsync(string job, ScheduledRun run, DateTimeOffset actualFireTime, UserId? triggeredBy, string instance, string? traceId, CancellationToken ct)
    {
        var id = Guid.CreateVersion7();
        await using var command = Database.CreateCommand(
            $"""
            INSERT INTO {SchedulingTables.Runs} ("Id", "JobName", "ScheduledFireTime", "ActualFireTime", "Trigger", "TriggeredBy", "Status", "InstanceId", "TraceId")
            VALUES (@id, @job, @scheduled, @actual, @trigger, @by, {ScheduledRunStatus.Running}, @instance, @trace)
            """);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("job", job);
        command.Parameters.AddWithValue("scheduled", run.ScheduledFireTime);
        command.Parameters.AddWithValue("actual", actualFireTime);
        command.Parameters.AddWithValue("trigger", (short)run.Trigger);
        command.Parameters.AddWithValue("by", (object?)triggeredBy?.Value ?? DBNull.Value);
        command.Parameters.AddWithValue("instance", instance);
        command.Parameters.AddWithValue("trace", (object?)traceId ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(ct);
        return id;
    }

    public async Task FinishAsync(Guid id, short status, ScheduledJobOutcome? outcome, string? errorCode, string? errorMessage, CancellationToken ct)
    {
        await using var command = Database.CreateCommand(
            $"""
            UPDATE {SchedulingTables.Runs}
            SET "Status" = @status, "FinishedAt" = clock_timestamp(),
                "DurationMs" = (extract(epoch FROM clock_timestamp() - "StartedAt") * 1000)::bigint,
                "ItemsProcessed" = @items, "Note" = left(@note, 1000), "ErrorCode" = @code, "ErrorMessage" = left(@message, 2000)
            WHERE "Id" = @id AND "Status" = {ScheduledRunStatus.Running}
            """);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue("items", (object?)outcome?.ItemsProcessed ?? DBNull.Value);
        command.Parameters.Add(new NpgsqlParameter<string?>("note", outcome?.Note));
        command.Parameters.Add(new NpgsqlParameter<string?>("code", errorCode));
        command.Parameters.Add(new NpgsqlParameter<string?>("message", errorMessage));
        await command.ExecuteNonQueryAsync(ct);
    }
}

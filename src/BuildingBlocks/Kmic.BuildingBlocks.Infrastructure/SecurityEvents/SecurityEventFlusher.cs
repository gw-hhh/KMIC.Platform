using System.Data.Common;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

internal sealed class SecurityEventFlusher(
    SecurityEventQueue queue,
    DataSources dataSources,
    IOptions<SecurityEventOptions> options,
    ILogger<SecurityEventFlusher> logger) : BackgroundService
{
    private const int MaxAttempts = 3;

    private const string InsertSql = $"""
        INSERT INTO {SecurityEventTable.Name}
            ("Id", "OccurredAt", "Type", "Severity", "Reason", "UserId", "ActualUserId", "RoleId", "SessionId",
             "AttemptedAccount", "ResourceType", "ResourceId", "Details", "TraceId")
        VALUES (@id, @at, @type, @severity, @reason, @user, @actual, @role, @session, @account, @resourceType, @resourceId, @details, @trace)
        """;

    private SecurityEventOptions Options => options.Value;

    public async Task FlushAsync()
    {
        var batch = new List<SecurityEventRecord>(Options.BatchSize);
        while (queue.Reader.TryRead(out var record))
        {
            batch.Add(record);
            if (batch.Count >= Options.BatchSize)
            {
                await WriteWithRetryAsync(batch);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await WriteWithRetryAsync(batch);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (await queue.Reader.WaitToReadAsync(stoppingToken))
            {
                // 若待写入队列尚未凑满整批，则短暂等待以聚合更多事件批量落库
                if (queue.Reader.Count < Options.BatchSize)
                {
                    await Task.Delay(Options.FlushInterval, stoppingToken);
                }

                await FlushAsync();
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }

        // 停机阶段须排空并持久化队列中剩余的全部安全事件
        await FlushAsync();
    }

    private async Task WriteWithRetryAsync(List<SecurityEventRecord> batch)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await WriteAsync(batch);
                return;
            }
            catch (Exception exception) when (exception is DbException or IOException or InvalidOperationException)
            {
                SecurityEventLog.WriteFailed(logger, exception, batch.Count, attempt);
                if (attempt < MaxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(attempt));
                }
            }
        }

        SecurityEventLog.GaveUp(logger, batch.Count);
    }

    // 单批次事件在同一隐式事务中写入，确保整批提交或整批失败
    private async Task WriteAsync(List<SecurityEventRecord> records)
    {
        await using var connection = await dataSources.Transactions(CallOrigin.BackgroundJob).OpenConnectionAsync();
        await using var batch = new NpgsqlBatch(connection);
        foreach (var record in records)
        {
            var command = new NpgsqlBatchCommand(InsertSql);
            command.Parameters.AddWithValue("id", record.Id);
            command.Parameters.AddWithValue("at", record.OccurredAt);
            command.Parameters.AddWithValue("type", record.Type);
            command.Parameters.AddWithValue("severity", record.Severity);
            Add(command, "reason", NpgsqlDbType.Varchar, record.Reason);
            Add(command, "user", NpgsqlDbType.Uuid, record.UserId);
            Add(command, "actual", NpgsqlDbType.Uuid, record.ActualUserId);
            Add(command, "role", NpgsqlDbType.Uuid, record.RoleId);
            Add(command, "session", NpgsqlDbType.Uuid, record.SessionId);
            Add(command, "account", NpgsqlDbType.Varchar, record.AttemptedAccount);
            Add(command, "resourceType", NpgsqlDbType.Varchar, record.ResourceType);
            Add(command, "resourceId", NpgsqlDbType.Uuid, record.ResourceId);
            Add(command, "details", NpgsqlDbType.Jsonb, record.Details);
            command.Parameters.AddWithValue("trace", record.TraceId);
            batch.BatchCommands.Add(command);
        }

        await batch.ExecuteNonQueryAsync();
    }

    private static void Add(NpgsqlBatchCommand command, string name, NpgsqlDbType type, object? value) =>
        command.Parameters.Add(new NpgsqlParameter(name, type) { Value = value ?? DBNull.Value });
}

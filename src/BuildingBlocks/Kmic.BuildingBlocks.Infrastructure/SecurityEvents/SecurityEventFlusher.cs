using System.Data.Common;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

/// <summary>
/// 把队列里的安全事件批量写进数据库
/// </summary>
/// <remarks>
/// 用后台数据源的独立连接，不占请求的事务连接，也不随业务事务回滚。每秒或攒够一批就写一次；
/// 停机只打断等待，已经从队列里取出的一批要写完，队列里剩下的也在退出前写完。
/// 数据库暂时写不进去就重试几次，仍然失败的丢弃并记错误日志
/// </remarks>
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

    /// <summary>
    /// 把队列里现有的事件全部写完
    /// </summary>
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
                // 队列还不满一批就等一会儿，让一次写入带上更多的事件
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

        // 停机时队列里剩下的也要写完
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

    // 一批在同一个隐式事务里写入，要么都进去要么都不进去
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

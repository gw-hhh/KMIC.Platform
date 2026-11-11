using System.Data.Common;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 把一个模块 Outbox 里的消息复制到各订阅方模块的 Inbox
/// </summary>
/// <remarks>
/// 每个订阅方模块单独一个事务写入。写 Inbox 前在同一事务里锁住并复核来源行，
/// 这样已经被别人接管、标记或清理的消息，任何迟到的投递者都写不进 Inbox
/// </remarks>
internal sealed class OutboxDispatcher(
    ModuleFrameworkTables tables,
    DataSources dataSources,
    IOptions<MessagingOptions> options,
    ILogger<OutboxDispatcher> logger)
{
    private NpgsqlDataSource Database => dataSources.Transactions(CallOrigin.Inbox);

    /// <summary>
    /// 领取一批并投递，返回领取到的条数
    /// </summary>
    public async Task<int> DeliverBatchAsync(string module, CancellationToken ct)
    {
        var token = Guid.CreateVersion7();
        var claimed = await ClaimAsync(module, options.Value.OutboxBatchSize, token, ct);
        if (claimed.Count == 0)
        {
            return 0;
        }

        var routes = await new MessageRoutes(Database).ListDeliveringAsync(ct);
        foreach (var message in claimed)
        {
            await DeliverAsync(module, message, token, routes, ct);
        }

        return claimed.Count;
    }

    public async Task<IReadOnlyList<ClaimedOutboxMessage>> ClaimAsync(string module, int max, Guid token, CancellationToken ct)
    {
        var outbox = tables.For(module).Outbox;
        await using var command = Database.CreateCommand(
            $"""
            WITH c AS (
                SELECT "Id" FROM {outbox}
                WHERE ("Status" = {MessageStatus.Pending} AND "NextAttemptAt" <= clock_timestamp())
                   OR ("Status" = {MessageStatus.Processing} AND "LeaseUntil" < clock_timestamp())
                ORDER BY "NextAttemptAt"
                LIMIT @max
                FOR UPDATE SKIP LOCKED)
            UPDATE {outbox} o
            SET "Status" = {MessageStatus.Processing}, "LeaseToken" = @token, "LeaseUntil" = clock_timestamp() + @lease, "Attempts" = o."Attempts" + 1
            FROM c WHERE o."Id" = c."Id"
            RETURNING o."Id", o."Seq", o."MessageType", o."SchemaVersion", o."Payload"::text, o."Headers"::text,
                o."SourceId", o."SourceVersion", o."OrderingKey", o."SubjectIds", o."OccurredAt", o."Attempts"
            """);
        command.Parameters.AddWithValue("max", max);
        command.Parameters.AddWithValue("token", token);
        command.Parameters.AddWithValue("lease", options.Value.OutboxLease);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var claimed = new List<ClaimedOutboxMessage>();
        while (await reader.ReadAsync(ct))
        {
            claimed.Add(new ClaimedOutboxMessage
            {
                Id = reader.GetGuid(0),
                Seq = reader.GetInt64(1),
                MessageType = reader.GetString(2),
                SchemaVersion = reader.GetInt32(3),
                Payload = reader.GetString(4),
                Headers = reader.GetString(5),
                SourceId = await reader.GetFieldValueAsync<Guid?>(6, ct),
                SourceVersion = await reader.GetFieldValueAsync<long?>(7, ct),
                OrderingKey = await reader.IsDBNullAsync(8, ct) ? null : reader.GetString(8),
                SubjectIds = await reader.IsDBNullAsync(9, ct) ? null : await reader.GetFieldValueAsync<Guid[]>(9, ct),
                OccurredAt = await reader.GetFieldValueAsync<DateTimeOffset>(10, ct),
                Attempts = reader.GetInt32(11),
            });
        }

        return claimed;
    }

    public async Task<DeliveryResult> DeliverAsync(string module, ClaimedOutboxMessage message, Guid token, IReadOnlyList<MessageRoute> routes, CancellationToken ct)
    {
        var targets = routes
            .Where(r => string.Equals(r.MessageType, message.MessageType, StringComparison.Ordinal))
            .GroupBy(r => r.Module, StringComparer.Ordinal)
            .ToList();
        if (targets.FirstOrDefault(t => !tables.Contains(t.Key)) is { } unknown)
        {
            return await RecordFailureAsync(module, message, token, $"订阅方模块 {unknown.Key} 没有在本实例注册。", null, ct);
        }

        try
        {
            foreach (var target in targets)
            {
                if (!await WriteInboxAsync(module, message, token, target.Key, [.. target.Select(r => r.Subscription)], ct))
                {
                    MessagingLog.DeliveryTakenOver(logger, message.Id, module);
                    return DeliveryResult.TakenOver;
                }
            }
        }
        catch (DbException exception)
        {
            return await RecordFailureAsync(module, message, token, exception.Message, exception, ct);
        }

        // 没有订阅方的消息也走到这里，直接标记已投递
        return await MarkDeliveredAsync(module, message, token, ct) ? DeliveryResult.Delivered : DeliveryResult.TakenOver;
    }

    private async Task<bool> WriteInboxAsync(string module, ClaimedOutboxMessage message, Guid token, string target, string[] subscriptions, CancellationToken ct)
    {
        await using var connection = await Database.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        // 锁到提交：复核通过之后，这一行不会被接管、标记或清理
        await using (var recheck = new NpgsqlCommand(
            $"""
            SELECT 1 FROM {tables.For(module).Outbox}
            WHERE "Id" = @id AND "Status" = {MessageStatus.Processing} AND "LeaseToken" = @token
            FOR SHARE
            """,
            connection,
            transaction))
        {
            recheck.Parameters.AddWithValue("id", message.Id);
            recheck.Parameters.AddWithValue("token", token);
            if (await recheck.ExecuteScalarAsync(ct) is null)
            {
                return false;
            }
        }

        foreach (var subscription in subscriptions)
        {
            await using var insert = new NpgsqlCommand(
                $"""
                INSERT INTO {tables.For(target).Inbox} ("Id", "MessageId", "Subscription", "MessageType", "SchemaVersion", "Payload", "Headers",
                    "SourceModule", "SourceId", "SourceVersion", "OrderingKey", "Seq", "SubjectIds", "OccurredAt", "Status", "Attempts")
                VALUES (@rowId, @id, @subscription, @type, @version, @payload, @headers,
                    @source, @sourceId, @sourceVersion, @orderingKey, @seq, @subjects, @occurredAt, {MessageStatus.Pending}, 0)
                ON CONFLICT ("MessageId", "Subscription") DO NOTHING
                """,
                connection,
                transaction);
            insert.Parameters.AddWithValue("rowId", Guid.CreateVersion7());
            insert.Parameters.AddWithValue("id", message.Id);
            insert.Parameters.AddWithValue("subscription", subscription);
            insert.Parameters.AddWithValue("type", message.MessageType);
            insert.Parameters.AddWithValue("version", message.SchemaVersion);
            insert.Parameters.AddWithValue("payload", NpgsqlDbType.Jsonb, message.Payload);
            insert.Parameters.AddWithValue("headers", NpgsqlDbType.Jsonb, message.Headers);
            insert.Parameters.AddWithValue("source", module);
            insert.Parameters.AddWithValue("sourceId", (object?)message.SourceId ?? DBNull.Value);
            insert.Parameters.AddWithValue("sourceVersion", (object?)message.SourceVersion ?? DBNull.Value);
            insert.Parameters.AddWithValue("orderingKey", (object?)message.OrderingKey ?? DBNull.Value);
            insert.Parameters.AddWithValue("seq", message.Seq);
            insert.Parameters.AddWithValue("subjects", NpgsqlDbType.Array | NpgsqlDbType.Uuid, (object?)message.SubjectIds ?? DBNull.Value);
            insert.Parameters.AddWithValue("occurredAt", message.OccurredAt);
            await insert.ExecuteNonQueryAsync(ct);
        }

        await using (var notify = new NpgsqlCommand("SELECT pg_notify(@channel, @module)", connection, transaction))
        {
            notify.Parameters.AddWithValue("channel", MessagingChannels.Inbox);
            notify.Parameters.AddWithValue("module", target);
            await notify.ExecuteNonQueryAsync(ct);
        }

        await transaction.CommitAsync(ct);
        return true;
    }

    private async Task<bool> MarkDeliveredAsync(string module, ClaimedOutboxMessage message, Guid token, CancellationToken ct)
    {
        await using var command = Database.CreateCommand(
            $"""
            UPDATE {tables.For(module).Outbox} SET "Status" = {MessageStatus.Done}, "DeliveredAt" = clock_timestamp()
            WHERE "Id" = @id AND "Status" = {MessageStatus.Processing} AND "LeaseToken" = @token
            """);
        command.Parameters.AddWithValue("id", message.Id);
        command.Parameters.AddWithValue("token", token);
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }

    private async Task<DeliveryResult> RecordFailureAsync(string module, ClaimedOutboxMessage message, Guid token, string error, Exception? exception, CancellationToken ct)
    {
        var settings = options.Value;
        var dead = message.Attempts >= settings.MaxAttempts;
        await using var command = Database.CreateCommand(
            $"""
            UPDATE {tables.For(module).Outbox}
            SET "Status" = @status, "NextAttemptAt" = clock_timestamp() + @delay, "LeaseUntil" = NULL, "LastError" = @error
            WHERE "Id" = @id AND "Status" = {MessageStatus.Processing} AND "LeaseToken" = @token
            """);
        command.Parameters.AddWithValue("status", dead ? MessageStatus.Dead : MessageStatus.Pending);
        command.Parameters.AddWithValue("delay", RetrySchedule.Delay(message.Attempts, settings));
        command.Parameters.AddWithValue("error", error.Length > 4000 ? error[..4000] : error);
        command.Parameters.AddWithValue("id", message.Id);
        command.Parameters.AddWithValue("token", token);
        if (await command.ExecuteNonQueryAsync(ct) == 0)
        {
            return DeliveryResult.TakenOver;
        }

        if (dead)
        {
            MessagingLog.DeliveryDead(logger, message.Id, message.MessageType, module, message.Attempts);
        }
        else
        {
            MessagingLog.DeliveryFailed(logger, exception, message.Id, message.MessageType, module, message.Attempts);
        }

        return dead ? DeliveryResult.Dead : DeliveryResult.Failed;
    }
}

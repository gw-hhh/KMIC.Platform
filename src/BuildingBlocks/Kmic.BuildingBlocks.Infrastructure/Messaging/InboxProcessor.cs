using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 处理一个模块 Inbox 里的消息
/// </summary>
/// <remarks>
/// 每条消息转成订阅声明的命令，以系统身份经完整的管道执行。“已处理”和业务数据在同一个事务里提交，
/// 占用被别的处理者接管时整个命令回滚，所以业务效果只有一次
/// </remarks>
internal sealed class InboxProcessor(
    ModuleFrameworkTables tables,
    DataSources dataSources,
    SubscriptionCatalog subscriptions,
    IExecutionScopeFactory scopes,
    IOptions<MessagingOptions> options,
    ILogger<InboxProcessor> logger)
{
    private NpgsqlDataSource Database => dataSources.Transactions(CallOrigin.Inbox);

    /// <summary>
    /// 领取一批并处理，返回领取到的条数
    /// </summary>
    public async Task<int> ProcessBatchAsync(string module, CancellationToken ct)
    {
        var token = Guid.CreateVersion7();
        var claimed = await ClaimAsync(module, options.Value.InboxBatchSize, token, ct);
        foreach (var message in claimed)
        {
            await ProcessAsync(module, message, token, ct);
        }

        return claimed.Count;
    }

    public async Task<IReadOnlyList<ClaimedInboxMessage>> ClaimAsync(string module, int max, Guid token, CancellationToken ct)
    {
        var own = subscriptions.All.Where(s => string.Equals(s.Module, module, StringComparison.Ordinal)).ToList();
        if (own.Count == 0)
        {
            return [];
        }

        // 只领取本实例代码认识的订阅：新订阅的消息留给新版本的实例处理
        // 有序订阅：同一个键前面还有没完成或进了死信的消息时，这条先不领
        var inbox = tables.For(module).Inbox;
        await using var command = Database.CreateCommand(
            $"""
            WITH c AS (
                SELECT i."Id" FROM {inbox} i
                WHERE i."Subscription" = ANY(@mine)
                  AND ((i."Status" = {MessageStatus.Pending} AND i."NextAttemptAt" <= clock_timestamp())
                    OR (i."Status" = {MessageStatus.Processing} AND i."LeaseUntil" < clock_timestamp()))
                  AND (i."Subscription" <> ALL(@ordered) OR i."OrderingKey" IS NULL OR NOT EXISTS (
                      SELECT 1 FROM {inbox} e
                      WHERE e."Subscription" = i."Subscription" AND e."OrderingKey" = i."OrderingKey"
                        AND e."Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing}, {MessageStatus.Dead}) AND e."Seq" < i."Seq"))
                ORDER BY i."NextAttemptAt"
                LIMIT @max
                FOR UPDATE SKIP LOCKED)
            UPDATE {inbox} m
            SET "Status" = {MessageStatus.Processing}, "LeaseToken" = @token, "LeaseUntil" = clock_timestamp() + @lease, "Attempts" = m."Attempts" + 1
            FROM c WHERE m."Id" = c."Id"
            RETURNING m."Id", m."MessageId", m."Subscription", m."MessageType", m."SchemaVersion", m."Payload"::text, m."Headers"::text,
                m."SourceModule", m."SourceId", m."SourceVersion", m."OccurredAt", m."Attempts"
            """);
        command.Parameters.AddWithValue("mine", own.Select(s => s.Name).Distinct(StringComparer.Ordinal).ToArray());
        command.Parameters.AddWithValue("ordered", own.Where(s => s.IsOrdered).Select(s => s.Name).Distinct(StringComparer.Ordinal).ToArray());
        command.Parameters.AddWithValue("max", max);
        command.Parameters.AddWithValue("token", token);
        command.Parameters.AddWithValue("lease", options.Value.InboxLease);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var claimed = new List<ClaimedInboxMessage>();
        while (await reader.ReadAsync(ct))
        {
            claimed.Add(new ClaimedInboxMessage
            {
                Id = reader.GetGuid(0),
                MessageId = reader.GetGuid(1),
                Subscription = reader.GetString(2),
                MessageType = reader.GetString(3),
                SchemaVersion = reader.GetInt32(4),
                Payload = reader.GetString(5),
                Headers = reader.GetString(6),
                SourceModule = reader.GetString(7),
                SourceId = await reader.GetFieldValueAsync<Guid?>(8, ct),
                SourceVersion = await reader.GetFieldValueAsync<long?>(9, ct),
                OccurredAt = await reader.GetFieldValueAsync<DateTimeOffset>(10, ct),
                Attempts = reader.GetInt32(11),
            });
        }

        return claimed;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "处理器的任何异常都只影响这一条消息，记为可重试的失败，循环继续处理别的消息")]
    public async Task<InboxResult> ProcessAsync(string module, ClaimedInboxMessage message, Guid token, CancellationToken ct)
    {
        if (subscriptions.Find(message.Subscription, message.SchemaVersion) is not { } subscription)
        {
            return await RecordFailureAsync(module, message, token, $"本实例的订阅 {message.Subscription} 不处理版本 {message.SchemaVersion}。", dead: true, null, ct);
        }

        object payload;
        MessageHeaders headers;
        try
        {
            payload = JsonSerializer.Deserialize(message.Payload, subscription.Message.ClrType, KmicJsonOptions.Stored) ?? throw new JsonException("消息内容是 null。");
            headers = JsonSerializer.Deserialize<MessageHeaders>(message.Headers, KmicJsonOptions.Stored) ?? throw new JsonException("信封是 null。");
        }
        catch (JsonException exception)
        {
            return await RecordFailureAsync(module, message, token, $"消息内容读不出来：{exception.Message}", dead: true, exception, ct);
        }

        var envelope = new MessageEnvelope(
            message.MessageId, message.MessageType, message.SchemaVersion, message.SourceModule, message.OccurredAt,
            message.SourceId, message.SourceVersion, headers.Purged, headers.InitiatorUserId);
        var initiator = headers.InitiatorUserId is { } id ? UserId.From(id) : (UserId?)null;

        await using var scope = scopes.CreateScope(CallOrigin.Inbox, CurrentUser.System(initiator));
        var current = scope.ServiceProvider.GetRequiredService<CurrentInboxMessage>();
        current.MessageId = message.MessageId;
        scope.ServiceProvider.GetRequiredService<CommandTransaction>().BeforeCommit(async (db, _, hookCt) =>
        {
            if (await MarkProcessedAsync(db, module, message, token, hookCt))
            {
                return null;
            }

            current.TakenOver = true;
            return CommonErrors.RequestInProgress;
        });

        Result result;
        try
        {
            result = await scope.ServiceProvider.GetRequiredService<ISender>().SendAsync(subscription.Map(payload, envelope), ct);
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            return await RecordFailureAsync(module, message, token, exception.Message, dead: false, exception, ct);
        }

        if (result.IsSuccess)
        {
            return InboxResult.Processed;
        }

        if (current.TakenOver)
        {
            return InboxResult.TakenOver;
        }

        // 暂时不满足的会自己好起来，按退避重试；其余业务失败重试也没用，直接进死信等人看
        return await RecordFailureAsync(module, message, token, $"{result.Error.Code}：{result.Error.Message}", dead: !IsRetryable(result.Error), null, ct);
    }

    private static bool IsRetryable(Error error) =>
        error.Type == ErrorType.Unavailable
        || string.Equals(error.Code, CommonErrors.VersionConflict.Code, StringComparison.Ordinal)
        || string.Equals(error.Code, CommonErrors.RequestInProgress.Code, StringComparison.Ordinal);

    private async Task<bool> MarkProcessedAsync(ModuleDbContext db, string module, ClaimedInboxMessage message, Guid token, CancellationToken ct)
    {
        var sql = $"""
            UPDATE {tables.For(module).Inbox} SET "Status" = {MessageStatus.Done}, "ProcessedAt" = clock_timestamp()
            WHERE "Id" = @id AND "Status" = {MessageStatus.Processing} AND "LeaseToken" = @token
            """;
        return await db.Database.ExecuteSqlRawAsync(sql, [new NpgsqlParameter("id", message.Id), new NpgsqlParameter("token", token)], ct) == 1;
    }

    public async Task<InboxResult> RecordFailureAsync(
        string module, ClaimedInboxMessage message, Guid token, string error, bool dead, Exception? exception, CancellationToken ct)
    {
        var settings = options.Value;
        dead = dead || message.Attempts >= settings.MaxAttempts;
        var inbox = tables.For(module).Inbox;
        try
        {
            await using var command = Database.CreateCommand(
                $"""
                UPDATE {inbox} SET "Status" = @status, "NextAttemptAt" = clock_timestamp() + @delay, "LeaseUntil" = NULL, "LastError" = @error
                WHERE "Id" = @id AND "Status" = {MessageStatus.Processing} AND "LeaseToken" = @token
                """);
            command.Parameters.AddWithValue("status", dead ? MessageStatus.Dead : MessageStatus.Pending);
            command.Parameters.AddWithValue("delay", RetrySchedule.Delay(message.Attempts, settings));
            command.Parameters.AddWithValue("error", error.Length > 4000 ? error[..4000] : error);
            command.Parameters.AddWithValue("id", message.Id);
            command.Parameters.AddWithValue("token", token);
            if (await command.ExecuteNonQueryAsync(ct) == 0)
            {
                return await ClassifyAsync(inbox, message, token, ct);
            }
        }
        catch (DbException failure)
        {
            MessagingLog.HandlingOutcomeUnknown(logger, failure, message.MessageId, message.Subscription);
            return InboxResult.Unknown;
        }

        if (dead)
        {
            MessagingLog.HandlingDead(logger, message.MessageId, message.Subscription, message.Attempts, error);
            return InboxResult.Dead;
        }

        MessagingLog.HandlingFailed(logger, exception, message.MessageId, message.Subscription, message.Attempts, error);
        return InboxResult.Failed;
    }

    // 记录失败影响了 0 行：重新读出来看实际状态，不猜测，也不重新执行
    private async Task<InboxResult> ClassifyAsync(string inbox, ClaimedInboxMessage message, Guid token, CancellationToken ct)
    {
        await using var command = Database.CreateCommand($"""SELECT "Status", "LeaseToken" FROM {inbox} WHERE "Id" = @id""");
        command.Parameters.AddWithValue("id", message.Id);
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return InboxResult.Unknown;
        }

        var status = reader.GetInt16(0);
        var holder = await reader.GetFieldValueAsync<Guid?>(1, ct);
        return status switch
        {
            MessageStatus.Done => InboxResult.Processed,
            MessageStatus.Processing when holder != token => InboxResult.TakenOver,
            _ => InboxResult.Unknown,
        };
    }
}

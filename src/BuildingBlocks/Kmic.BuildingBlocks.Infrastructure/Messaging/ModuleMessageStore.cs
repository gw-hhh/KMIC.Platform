using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// <see cref="IModuleMessageStore" /> 的实现：直接读写一个模块的发件箱与收件箱表
/// </summary>
/// <remarks>
/// 读走读池，管理视图允许几百毫秒的复制延迟；写走事务池。所有写入都带“状态是死信”的条件，
/// 与领取、处理的状态条件互不覆盖：处理完成要求处理中且令牌匹配，这里只改死信
/// </remarks>
internal sealed class ModuleMessageStore(string module, ModuleFrameworkTables tables, DataSources dataSources, SubscriptionCatalog catalog) : IModuleMessageStore
{
    private const int MaxBlockedKeys = 500;

    private FrameworkTableNames Tables => tables.For(module);

    private NpgsqlDataSource Reads => dataSources.Reads(CallOrigin.Http);

    private NpgsqlDataSource Writes => dataSources.Transactions(CallOrigin.Http);

    public string Module => module;

    public async Task<ModuleMessageBacklog> GetBacklogAsync(CancellationToken ct)
    {
        await using var outboxCommand = Reads.CreateCommand(
            $"""
            SELECT count(*) FILTER (WHERE "Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing})),
                   COALESCE(extract(epoch FROM clock_timestamp() - min("OccurredAt") FILTER (WHERE "Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing}))), 0)::float8,
                   count(*) FILTER (WHERE "Status" = {MessageStatus.Dead})
            FROM {Tables.Outbox}
            WHERE "Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing}, {MessageStatus.Dead})
            """);
        await using var outboxReader = await outboxCommand.ExecuteReaderAsync(ct);
        await outboxReader.ReadAsync(ct);
        var outbox = new OutboxBacklogInfo(outboxReader.GetInt64(0), TimeSpan.FromSeconds(outboxReader.GetDouble(1)), outboxReader.GetInt64(2));

        await using var inboxCommand = Reads.CreateCommand(
            $"""
            SELECT "Subscription",
                   count(*) FILTER (WHERE "Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing})),
                   COALESCE(extract(epoch FROM clock_timestamp() - min("ReceivedAt") FILTER (WHERE "Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing}))), 0)::float8,
                   count(*) FILTER (WHERE "Status" = {MessageStatus.Dead}),
                   count(*) FILTER (WHERE "Status" = {MessageStatus.Abandoned}),
                   count(DISTINCT "OrderingKey") FILTER (WHERE "Status" = {MessageStatus.Dead})
            FROM {Tables.Inbox}
            WHERE "Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing}, {MessageStatus.Dead}, {MessageStatus.Abandoned})
            GROUP BY "Subscription"
            """);
        await using var inboxReader = await inboxCommand.ExecuteReaderAsync(ct);
        var rows = new Dictionary<string, SubscriptionBacklogInfo>(StringComparer.Ordinal);
        while (await inboxReader.ReadAsync(ct))
        {
            var subscription = inboxReader.GetString(0);
            rows[subscription] = new SubscriptionBacklogInfo(
                subscription,
                inboxReader.GetInt64(1),
                TimeSpan.FromSeconds(inboxReader.GetDouble(2)),
                inboxReader.GetInt64(3),
                inboxReader.GetInt64(4),
                catalog.All.Any(s => s.IsOrdered && string.Equals(s.Name, subscription, StringComparison.Ordinal)) ? inboxReader.GetInt64(5) : 0);
        }

        // 本实例声明了、但收件箱里还没有任何行的订阅也列出来：看板上才分得清“没有积压”和“没有这个订阅”
        foreach (var declared in catalog.All.Where(s => string.Equals(s.Module, module, StringComparison.Ordinal)).Select(s => s.Name).Distinct(StringComparer.Ordinal))
        {
            rows.TryAdd(declared, new SubscriptionBacklogInfo(declared, 0, TimeSpan.Zero, 0, 0, 0));
        }

        return new ModuleMessageBacklog(module, outbox, [.. rows.Values.OrderBy(r => r.Subscription, StringComparer.Ordinal)]);
    }

    public async Task<IReadOnlyList<DeadMessage>> ListDeadAsync(DeadMessageFilter filter, MessagePosition? after, int take, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentOutOfRangeException.ThrowIfLessThan(take, 1);

        await using var command = Reads.CreateCommand(
            $"""
            SELECT d.* FROM (
                SELECT 0 AS box, "Id", NULL::text AS subscription, "Id" AS message_id, "MessageType", "SchemaVersion", "OccurredAt", "Attempts", "LastError"
                FROM {Tables.Outbox}
                WHERE "Status" = {MessageStatus.Dead} AND (@box::int IS NULL OR @box = 0) AND @subscription::text IS NULL AND (@type::text IS NULL OR "MessageType" = @type)
                UNION ALL
                SELECT 1, "Id", "Subscription", "MessageId", "MessageType", "SchemaVersion", "OccurredAt", "Attempts", "LastError"
                FROM {Tables.Inbox}
                WHERE "Status" = {MessageStatus.Dead} AND (@box::int IS NULL OR @box = 1) AND (@subscription::text IS NULL OR "Subscription" = @subscription) AND (@type::text IS NULL OR "MessageType" = @type)
            ) d
            WHERE @afterAt::timestamptz IS NULL OR (d."OccurredAt", d."Id") < (@afterAt, @afterId)
            ORDER BY d."OccurredAt" DESC, d."Id" DESC
            LIMIT @take
            """);
        command.Parameters.Add(new NpgsqlParameter("box", NpgsqlDbType.Integer) { Value = filter.Box is { } box ? (int)box : DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("subscription", NpgsqlDbType.Text) { Value = (object?)filter.Subscription ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("type", NpgsqlDbType.Text) { Value = (object?)filter.MessageType ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("afterAt", NpgsqlDbType.TimestampTz) { Value = after is { } position ? position.OccurredAt : DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("afterId", NpgsqlDbType.Uuid) { Value = after is { } last ? last.Id : Guid.Empty });
        command.Parameters.AddWithValue("take", take);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var messages = new List<DeadMessage>();
        while (await reader.ReadAsync(ct))
        {
            messages.Add(ReadDead(reader));
        }

        return messages;
    }

    public async Task<DeadMessageDetail?> GetAsync(MessageBox box, Guid id, CancellationToken ct)
    {
        var inbox = box == MessageBox.Inbox;
        await using var command = Reads.CreateCommand(
            inbox
                ? $"""
                  SELECT 1 AS box, "Id", "Subscription", "MessageId", "MessageType", "SchemaVersion", "OccurredAt", "Attempts", "LastError", "Status", "Payload"::text, "Headers"::text
                  FROM {Tables.Inbox} WHERE "Id" = @id
                  """
                : $"""
                  SELECT 0 AS box, "Id", NULL::text, "Id", "MessageType", "SchemaVersion", "OccurredAt", "Attempts", "LastError", "Status", "Payload"::text, "Headers"::text
                  FROM {Tables.Outbox} WHERE "Id" = @id
                  """);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        var state = StateOf(reader.GetInt16(9));
        return new DeadMessageDetail(ReadDead(reader), state, reader.GetString(10), reader.GetString(11));
    }

    public async Task<IReadOnlyList<BlockedKey>> ListBlockedKeysAsync(CancellationToken ct)
    {
        var ordered = catalog.All.Where(s => s.IsOrdered && string.Equals(s.Module, module, StringComparison.Ordinal)).Select(s => s.Name).Distinct(StringComparer.Ordinal).ToList();
        if (ordered.Count == 0)
        {
            return [];
        }

        await using var command = Reads.CreateCommand(
            $"""
            WITH dead AS (
                SELECT "Subscription", "OrderingKey", min("Seq") AS first_seq, count(*) AS dead_count
                FROM {Tables.Inbox}
                WHERE "Subscription" = ANY(@ordered) AND "OrderingKey" IS NOT NULL AND "Status" = {MessageStatus.Dead}
                GROUP BY "Subscription", "OrderingKey")
            SELECT d."Subscription", d."OrderingKey", d.dead_count,
                   (SELECT count(*) FROM {Tables.Inbox} w
                    WHERE w."Subscription" = d."Subscription" AND w."OrderingKey" = d."OrderingKey"
                      AND w."Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing}) AND w."Seq" > d.first_seq) AS waiting,
                   f."Id", f."LastError"
            FROM dead d
            JOIN {Tables.Inbox} f ON f."Subscription" = d."Subscription" AND f."OrderingKey" = d."OrderingKey" AND f."Seq" = d.first_seq AND f."Status" = {MessageStatus.Dead}
            ORDER BY waiting DESC, d."Subscription", d."OrderingKey"
            LIMIT {MaxBlockedKeys}
            """);
        command.Parameters.AddWithValue("ordered", ordered.ToArray());
        await using var reader = await command.ExecuteReaderAsync(ct);
        var keys = new List<BlockedKey>();
        while (await reader.ReadAsync(ct))
        {
            keys.Add(new BlockedKey(
                module, reader.GetString(0), reader.GetString(1), reader.GetInt64(2), reader.GetInt64(3), reader.GetGuid(4), await reader.IsDBNullAsync(5, ct) ? null : reader.GetString(5)));
        }

        return keys;
    }

    public async Task<IReadOnlyList<MessageActionResult>> RetryAsync(MessageBox box, IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var table = box == MessageBox.Inbox ? Tables.Inbox : Tables.Outbox;
        await using var command = Writes.CreateCommand(
            $"""
            UPDATE {table}
            SET "Status" = {MessageStatus.Pending}, "Attempts" = 0, "NextAttemptAt" = clock_timestamp(), "LeaseToken" = NULL, "LeaseUntil" = NULL
            WHERE "Id" = ANY(@ids) AND "Status" = {MessageStatus.Dead}
            RETURNING "Id"
            """);
        command.Parameters.AddWithValue("ids", ids.Distinct().ToArray());
        return await ResultsAsync(command, table, ids, ct);
    }

    public async Task<IReadOnlyList<MessageActionResult>> AbandonAsync(IReadOnlyCollection<Guid> ids, string reason, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        // 收件箱的行没有单独的“放弃原因”列，原因连同原来的错误一起记进错误说明
        await using var command = Writes.CreateCommand(
            $"""
            UPDATE {Tables.Inbox}
            SET "Status" = {MessageStatus.Abandoned}, "LeaseToken" = NULL, "LeaseUntil" = NULL,
                "LastError" = left('放弃：' || @reason || '；此前的错误：' || COALESCE("LastError", '无'), 4000)
            WHERE "Id" = ANY(@ids) AND "Status" = {MessageStatus.Dead}
            RETURNING "Id"
            """);
        command.Parameters.AddWithValue("ids", ids.Distinct().ToArray());
        command.Parameters.AddWithValue("reason", reason);
        return await ResultsAsync(command, Tables.Inbox, ids, ct);
    }

    private static DeadMessageState StateOf(short status) =>
        status switch
        {
            MessageStatus.Dead => DeadMessageState.Dead,
            MessageStatus.Abandoned => DeadMessageState.Abandoned,
            _ => DeadMessageState.Other,
        };

    private DeadMessage ReadDead(NpgsqlDataReader reader) =>
        new(
            reader.GetGuid(1),
            reader.GetInt32(0) == 1 ? MessageBox.Inbox : MessageBox.Outbox,
            module,
            reader.IsDBNull(2) ? null : reader.GetString(2),
            reader.GetGuid(3),
            reader.GetString(4),
            reader.GetInt32(5),
            reader.GetFieldValue<DateTimeOffset>(6),
            reader.GetInt32(7),
            reader.IsDBNull(8) ? null : reader.GetString(8));

    private static MessageActionOutcome OutcomeOf(Guid id, HashSet<Guid> done, HashSet<Guid> existing)
    {
        if (done.Contains(id))
        {
            return MessageActionOutcome.Done;
        }

        return existing.Contains(id) ? MessageActionOutcome.WrongState : MessageActionOutcome.NotFound;
    }

    // 先执行 UPDATE，没改到的再查一次是不存在还是状态不对
    private async Task<IReadOnlyList<MessageActionResult>> ResultsAsync(NpgsqlCommand update, string table, IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        var done = new HashSet<Guid>();
        await using (var reader = await update.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                done.Add(reader.GetGuid(0));
            }
        }

        var rest = ids.Where(id => !done.Contains(id)).Distinct().ToArray();
        var existing = new HashSet<Guid>();
        if (rest.Length > 0)
        {
            await using var check = Writes.CreateCommand($"""SELECT "Id" FROM {table} WHERE "Id" = ANY(@ids)""");
            check.Parameters.AddWithValue("ids", rest);
            await using var reader = await check.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                existing.Add(reader.GetGuid(0));
            }
        }

        return [.. ids.Select(id => new MessageActionResult(id, OutcomeOf(id, done, existing)))];
    }
}

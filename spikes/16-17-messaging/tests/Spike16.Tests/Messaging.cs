using Npgsql;
using Xunit;

namespace Spike16.Tests;

// 05-07 第 6.2、7.3、9.2 节及 R02 补充说明的投递、处理、清理，按文档中的 SQL 实现
internal static class Messaging
{
    public const string Quotation = "quotation";
    public const string Orders = "orders";

    public static async Task<Guid> PublishAsync(NpgsqlDataSource db)
    {
        var id = Guid.CreateVersion7();
        await Db.ExecAsync(db, """
            INSERT INTO customer."OutboxMessage" ("Id", "MessageType", "Payload", "Status", "NextAttemptAt")
            VALUES (@id, 'Customer.CustomerChanged', '{"name":"示例"}', 0, clock_timestamp())
            """, ("id", id));
        return id;
    }

    // 短事务领取：待投递，或投递中但租约已过期
    public static async Task<IReadOnlyList<Claimed>> ClaimAsync(NpgsqlDataSource db, TimeSpan lease)
    {
        var token = Guid.CreateVersion7();
        var rows = await Db.QueryAsync(db, """
            WITH c AS (
                SELECT "Id" FROM customer."OutboxMessage"
                WHERE ("Status" = 0 AND "NextAttemptAt" <= clock_timestamp())
                   OR ("Status" = 1 AND "LeaseUntil" < clock_timestamp())
                ORDER BY "NextAttemptAt"
                LIMIT 10
                FOR UPDATE SKIP LOCKED)
            UPDATE customer."OutboxMessage" o
            SET "Status" = 1, "LeaseToken" = @token, "LeaseUntil" = clock_timestamp() + @lease, "Attempts" = o."Attempts" + 1
            FROM c WHERE o."Id" = c."Id"
            RETURNING o."Id", o."Payload"::text
            """, ("token", token), ("lease", lease));
        return [.. rows.Select(r => new Claimed((Guid)r[0]!, token, (string)r[1]!))];
    }

    // 写入一个订阅方的 Inbox。recheck = true 为 R02 修订后的写法：同一事务内先对来源行加共享锁并复核领取令牌
    public static async Task<bool> WriteInboxAsync(NpgsqlDataSource db, Claimed message, string subscriber, bool recheck = true)
    {
        await using var connection = await db.OpenConnectionAsync(TestContext.Current.CancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(TestContext.Current.CancellationToken);
        if (recheck && !await RecheckAsync(connection, transaction, message))
        {
            await transaction.RollbackAsync(TestContext.Current.CancellationToken);
            return false;
        }

        await InsertInboxAsync(connection, transaction, message, subscriber);
        await transaction.CommitAsync(TestContext.Current.CancellationToken);
        return true;
    }

    public static async Task<bool> RecheckAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Claimed message) =>
        (await Db.QueryAsync(connection, transaction, """
            SELECT 1 FROM customer."OutboxMessage"
            WHERE "Id" = @id AND "Status" = 1 AND "LeaseToken" = @token
            FOR SHARE
            """, ("id", message.Id), ("token", message.Token))).Count == 1;

    public static Task<int> InsertInboxAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Claimed message, string subscriber) =>
        Db.ExecAsync(connection, transaction, $"""
            INSERT INTO {Table(subscriber)} ("Id", "MessageId", "Subscription", "SourceModule", "Payload", "Status", "NextAttemptAt")
            VALUES (@rowId, @id, @subscription, 'customer', @payload::jsonb, 0, clock_timestamp())
            ON CONFLICT ("MessageId", "Subscription") DO NOTHING
            """, ("rowId", Guid.CreateVersion7()), ("id", message.Id), ("subscription", $"{subscriber}.SyncCustomer"), ("payload", message.Payload));

    public static Task<int> MarkDeliveredAsync(NpgsqlDataSource db, Claimed message) =>
        Db.ExecAsync(db, """
            UPDATE customer."OutboxMessage" SET "Status" = 2, "DeliveredAt" = clock_timestamp()
            WHERE "Id" = @id AND "Status" = 1 AND "LeaseToken" = @token
            """, ("id", message.Id), ("token", message.Token));

    // 投递失败：只作用于本次领取的行；达到最大次数进入死信
    public static Task<int> RecordDeliveryFailureAsync(NpgsqlDataSource db, Claimed message, int maxAttempts) =>
        Db.ExecAsync(db, """
            UPDATE customer."OutboxMessage"
            SET "Status" = CASE WHEN "Attempts" >= @max THEN 3 ELSE 0 END, "NextAttemptAt" = clock_timestamp(), "LastError" = '模拟故障'
            WHERE "Id" = @id AND "Status" = 1 AND "LeaseToken" = @token
            """, ("id", message.Id), ("token", message.Token), ("max", maxAttempts));

    public static Task<int> RetryDeadLetterAsync(NpgsqlDataSource db, Guid id) =>
        Db.ExecAsync(db, """
            UPDATE customer."OutboxMessage" SET "Status" = 0, "Attempts" = 0, "NextAttemptAt" = clock_timestamp()
            WHERE "Id" = @id AND "Status" = 3
            """, ("id", id));

    // 一次完整的投递：逐个订阅方写 Inbox，全部成功后标记已投递
    public static async Task<bool> DeliverAsync(NpgsqlDataSource db, Claimed message, string[] subscribers, string? failingSubscriber = null)
    {
        foreach (var subscriber in subscribers)
        {
            if (string.Equals(subscriber, failingSubscriber, StringComparison.Ordinal))
            {
                await RecordDeliveryFailureAsync(db, message, maxAttempts: 1);
                return false;
            }

            if (!await WriteInboxAsync(db, message, subscriber))
            {
                return false;
            }
        }

        return await MarkDeliveredAsync(db, message) == 1;
    }

    public static async Task<IReadOnlyList<ClaimedInbox>> ClaimInboxAsync(NpgsqlDataSource db, string subscriber)
    {
        var token = Guid.CreateVersion7();
        var rows = await Db.QueryAsync(db, $"""
            WITH c AS (
                SELECT "Id" FROM {Table(subscriber)}
                WHERE ("Status" = 0 AND "NextAttemptAt" <= clock_timestamp())
                   OR ("Status" = 1 AND "LeaseUntil" < clock_timestamp())
                LIMIT 10
                FOR UPDATE SKIP LOCKED)
            UPDATE {Table(subscriber)} i
            SET "Status" = 1, "LeaseToken" = @token, "LeaseUntil" = clock_timestamp() + interval '2 minutes', "Attempts" = i."Attempts" + 1
            FROM c WHERE i."Id" = c."Id"
            RETURNING i."Id", i."MessageId"
            """, ("token", token));
        return [.. rows.Select(r => new ClaimedInbox((Guid)r[0]!, (Guid)r[1]!, token))];
    }

    // 处理：业务效果与“已处理”同一事务；令牌不符即回滚
    public static async Task<bool> ProcessAsync(NpgsqlDataSource db, string subscriber, ClaimedInbox row)
    {
        await using var connection = await db.OpenConnectionAsync(TestContext.Current.CancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(TestContext.Current.CancellationToken);
        await Db.ExecAsync(connection, transaction, $"""INSERT INTO {subscriber}."Effect" ("MessageId") VALUES (@id)""", ("id", (object)row.MessageId));
        var marked = await Db.ExecAsync(connection, transaction, $"""
            UPDATE {Table(subscriber)} SET "Status" = 2, "ProcessedAt" = clock_timestamp()
            WHERE "Id" = @id AND "Status" = 1 AND "LeaseToken" = @token
            """, ("id", (object)row.Id), ("token", row.Token));
        if (marked == 0)
        {
            await transaction.RollbackAsync(TestContext.Current.CancellationToken);
            return false;
        }

        await transaction.CommitAsync(TestContext.Current.CancellationToken);
        return true;
    }

    public static async Task DrainInboxAsync(NpgsqlDataSource db, string subscriber)
    {
        foreach (var row in await ClaimInboxAsync(db, subscriber))
        {
            await ProcessAsync(db, subscriber, row);
        }
    }

    // 处理失败的记录。conditional = true 为 R02 修订后的写法
    public static Task<int> RecordProcessingFailureAsync(NpgsqlDataSource db, string subscriber, ClaimedInbox row, bool conditional) =>
        Db.ExecAsync(db, conditional
            ? $"""
              UPDATE {Table(subscriber)} SET "Status" = 0, "NextAttemptAt" = clock_timestamp(), "LastError" = '确认丢失'
              WHERE "Id" = @id AND "Status" = 1 AND "LeaseToken" = @token
              """
            : $"""
              UPDATE {Table(subscriber)} SET "Status" = 0, "NextAttemptAt" = clock_timestamp(), "LastError" = '确认丢失'
              WHERE "Id" = @id
              """, ("id", row.Id), ("token", row.Token));

    public static Task<short> InboxStatusAsync(NpgsqlDataSource db, string subscriber, Guid inboxId) =>
        Db.ScalarAsync<short>(db, $"""SELECT "Status" FROM {Table(subscriber)} WHERE "Id" = @id""", ("id", inboxId));

    // 清理。retentionContract = true 为 R02 的保留契约：去重凭证只在来源 Outbox 行删除之后才删
    public static async Task CleanupAsync(NpgsqlDataSource db, TimeSpan retention, bool retentionContract = true)
    {
        await Db.ExecAsync(db, """
            DELETE FROM customer."OutboxMessage" WHERE "Status" = 2 AND "DeliveredAt" < clock_timestamp() - @retention
            """, ("retention", retention));
        foreach (var subscriber in new[] { Quotation, Orders })
        {
            await Db.ExecAsync(db, retentionContract
                ? $"""
                  DELETE FROM {Table(subscriber)} i
                  WHERE i."Status" IN (2, 4) AND i."SourceModule" = 'customer'
                    AND NOT EXISTS (SELECT 1 FROM customer."OutboxMessage" o WHERE o."Id" = i."MessageId")
                  """
                : $"""
                  DELETE FROM {Table(subscriber)} WHERE "Status" IN (2, 4) AND "ProcessedAt" < clock_timestamp() - @retention
                  """, ("retention", retention));
        }
    }

    public static Task<long> EffectsAsync(NpgsqlDataSource db, string subscriber, Guid messageId) =>
        Db.ScalarAsync<long>(db, $"""SELECT count(*) FROM {subscriber}."Effect" WHERE "MessageId" = @id""", ("id", messageId));

    public static Task<long> InboxRowsAsync(NpgsqlDataSource db, string subscriber, Guid messageId) =>
        Db.ScalarAsync<long>(db, $"""SELECT count(*) FROM {Table(subscriber)} WHERE "MessageId" = @id""", ("id", messageId));

    private static string Table(string subscriber) => subscriber switch
    {
        Quotation => """quotation."InboxMessage" """,
        Orders => """orders."InboxMessage" """,
        _ => throw new ArgumentOutOfRangeException(nameof(subscriber)),
    };
}

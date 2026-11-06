using Npgsql;
using Xunit;

namespace Spike18.Tests;

// Files.PurgeExpired 对单个候选文件的协议：尝试取排他咨询锁 → 先探查 Outbox、再探查 Files Inbox 中未完成的 AttachFiles → 带状态条件改为 Deleting
internal sealed class CleanupTransaction : IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;

    private CleanupTransaction(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public static async Task<CleanupTransaction> BeginAsync(NpgsqlDataSource db)
    {
        var connection = await db.OpenConnectionAsync(TestContext.Current.CancellationToken);
        return new CleanupTransaction(connection, await connection.BeginTransactionAsync(TestContext.Current.CancellationToken));
    }

    public async Task<bool> TryLockAsync(Guid fileId) =>
        (bool)(await Db.QueryAsync(_connection, _transaction, "SELECT pg_try_advisory_xact_lock(@key)", ("key", FileLocks.Key(fileId))))[0][0]!;

    public async Task<bool> PendingInOutboxAsync(Guid fileId) =>
        (await Db.QueryAsync(_connection, _transaction, """
            SELECT 1 FROM quotation."OutboxMessage" WHERE "SubjectIds" && ARRAY[@file]::uuid[] AND "Status" IN (0, 1, 3) LIMIT 1
            """, ("file", fileId))).Count > 0;

    public async Task<bool> PendingInInboxAsync(Guid fileId) =>
        (await Db.QueryAsync(_connection, _transaction, """
            SELECT 1 FROM files."InboxMessage" WHERE "SubjectIds" && ARRAY[@file]::uuid[] AND "Status" IN (0, 1, 3) LIMIT 1
            """, ("file", fileId))).Count > 0;

    public async Task<bool> MarkDeletingAsync(Guid fileId) =>
        await Db.ExecAsync(_connection, _transaction, """
            UPDATE files."StoredFile" SET "Status" = 'Deleting' WHERE "Id" = @id AND "Status" IN ('Available', 'Detached')
            """, ("id", fileId)) == 1;

    public Task CommitAsync() => _transaction.CommitAsync(TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
    }

    // 完整的一次清理：候选按到期时间筛选，决定删不删的是锁 + 探查 + 状态条件
    public static async Task<IReadOnlyList<Guid>> PurgeExpiredAsync(NpgsqlDataSource db)
    {
        await using var cleanup = await BeginAsync(db);
        var candidates = await Db.QueryAsync(cleanup._connection, cleanup._transaction, """
            SELECT "Id" FROM files."StoredFile" WHERE "Status" IN ('Available', 'Detached') AND "ExpiresAt" < clock_timestamp() LIMIT 1000
            """);
        var marked = new List<Guid>();
        foreach (var fileId in candidates.Select(r => (Guid)r[0]!))
        {
            if (await cleanup.TryLockAsync(fileId)
                && !await cleanup.PendingInOutboxAsync(fileId)
                && !await cleanup.PendingInInboxAsync(fileId)
                && await cleanup.MarkDeletingAsync(fileId))
            {
                marked.Add(fileId);
            }
        }

        await cleanup.CommitAsync();
        return marked;
    }

    // 对照组：R02 之前的写法，只按到期时间判断
    public static Task<int> PurgeExpiredByTimeAsync(NpgsqlDataSource db) =>
        Db.ExecAsync(db, """
            UPDATE files."StoredFile" SET "Status" = 'Deleting' WHERE "Status" IN ('Available', 'Detached') AND "ExpiresAt" < clock_timestamp()
            """);
}

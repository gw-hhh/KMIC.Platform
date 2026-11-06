using Npgsql;
using Xunit;

namespace Spike18.Tests;

// 业务命令中 IFileAttachments.AttachAsync 的协议：按文件 ID 排序取事务级共享咨询锁 → 读 Files 的最新状态 → 写关联行与 AttachFiles → 提交
internal sealed class AttachTransaction : IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;
    private readonly IReadOnlyList<Guid> _fileIds;

    private AttachTransaction(NpgsqlConnection connection, NpgsqlTransaction transaction, IReadOnlyList<Guid> fileIds)
    {
        _connection = connection;
        _transaction = transaction;
        _fileIds = fileIds;
    }

    public int ProcessId => _connection.ProcessID;

    public static async Task<AttachTransaction> BeginAsync(NpgsqlDataSource db, IReadOnlyList<Guid> fileIds, bool waitForStandby, TimeSpan lockTimeout)
    {
        var connection = await db.OpenConnectionAsync(TestContext.Current.CancellationToken);
        var transaction = await connection.BeginTransactionAsync(TestContext.Current.CancellationToken);
        await Db.ExecAsync(connection, transaction, $"SET LOCAL lock_timeout = '{(int)lockTimeout.TotalMilliseconds}ms'");
        if (waitForStandby)
        {
            await Db.ExecAsync(connection, transaction, "SET LOCAL synchronous_commit = on");
        }

        return new AttachTransaction(connection, transaction, [.. fileIds.OrderBy(FileLocks.Key)]);
    }

    public async Task LockAsync()
    {
        foreach (var fileId in _fileIds)
        {
            await Db.ExecAsync(_connection, _transaction, "SELECT pg_advisory_xact_lock_shared(@key)", ("key", FileLocks.Key(fileId)));
        }
    }

    // Files Contracts 的可关联性检查：另一个连接（读池）读主实例的最新已提交状态；不检查到期时间
    public async Task<bool> CheckAttachableAsync(NpgsqlDataSource db)
    {
        var rows = await Db.QueryAsync(db, """
            SELECT count(*) FROM files."StoredFile" WHERE "Id" = ANY (@ids) AND "Status" IN ('Available', 'Detached')
            """, ("ids", _fileIds.ToArray()));
        return (long)rows[0][0]! == _fileIds.Count;
    }

    public async Task<Guid> WriteAsync(Guid quotationId)
    {
        foreach (var fileId in _fileIds)
        {
            await Db.ExecAsync(_connection, _transaction, """
                INSERT INTO quotation."QuotationAttachment" ("Id", "QuotationId", "FileId") VALUES (@id, @quotation, @file)
                """, ("id", Guid.CreateVersion7()), ("quotation", quotationId), ("file", fileId));
        }

        var messageId = Guid.CreateVersion7();
        await Db.ExecAsync(_connection, _transaction, """
            INSERT INTO quotation."OutboxMessage" ("Id", "MessageType", "Status", "SubjectIds") VALUES (@id, 'Files.AttachFiles', 0, @files)
            """, ("id", messageId), ("files", _fileIds.ToArray()));
        return messageId;
    }

    public Task CommitAsync() => _transaction.CommitAsync(TestContext.Current.CancellationToken);

    public Task RollbackAsync() => _transaction.RollbackAsync(TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

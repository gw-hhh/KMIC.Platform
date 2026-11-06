using Npgsql;

namespace Spike18.Tests;

// Files 模块的文件行，以及 AttachFiles 从 Outbox 到 Files Inbox 再到处理完成的三步
internal static class Files
{
    public static async Task<Guid> UploadAsync(NpgsqlDataSource db, TimeSpan expiresIn)
    {
        var id = Guid.CreateVersion7();
        await Db.ExecAsync(db, """
            INSERT INTO files."StoredFile" ("Id", "Status", "ExpiresAt") VALUES (@id, 'Available', clock_timestamp() + @expires)
            """, ("id", id), ("expires", expiresIn));
        return id;
    }

    public static Task<string> StatusAsync(NpgsqlDataSource db, Guid fileId) =>
        Db.ScalarAsync<string>(db, """SELECT "Status" FROM files."StoredFile" WHERE "Id" = @id""", ("id", fileId));

    public static Task<long> AttachmentsAsync(NpgsqlDataSource db, Guid fileId) =>
        Db.ScalarAsync<long>(db, """SELECT count(*) FROM quotation."QuotationAttachment" WHERE "FileId" = @id""", ("id", fileId));

    // 投递：写入 Files Inbox（与 05-07 相同，一个短事务）
    public static Task<int> DeliverToInboxAsync(NpgsqlDataSource db, Guid messageId) =>
        Db.ExecAsync(db, """
            INSERT INTO files."InboxMessage" ("Id", "MessageId", "Status", "SubjectIds")
            SELECT @row, "Id", 0, "SubjectIds" FROM quotation."OutboxMessage" WHERE "Id" = @id
            """, ("row", Guid.CreateVersion7()), ("id", messageId));

    public static Task<int> MarkOutboxDeliveredAsync(NpgsqlDataSource db, Guid messageId) =>
        Db.ExecAsync(db, """UPDATE quotation."OutboxMessage" SET "Status" = 2 WHERE "Id" = @id""", ("id", messageId));

    // 处理 AttachFiles：文件改为 Attached；返回实际改到的文件数（文件已是 Deleting 时为 0，即关联指向了一个要被删除的文件）
    public static async Task<int> ProcessAttachFilesAsync(NpgsqlDataSource db, Guid messageId)
    {
        await using var connection = await db.OpenConnectionAsync(Xunit.TestContext.Current.CancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(Xunit.TestContext.Current.CancellationToken);
        var attached = await Db.ExecAsync(connection, transaction, """
            UPDATE files."StoredFile" SET "Status" = 'Attached', "ExpiresAt" = NULL
            WHERE "Id" IN (SELECT unnest("SubjectIds") FROM files."InboxMessage" WHERE "MessageId" = @id) AND "Status" IN ('Available', 'Detached')
            """, ("id", messageId));
        await Db.ExecAsync(connection, transaction, """UPDATE files."InboxMessage" SET "Status" = 2 WHERE "MessageId" = @id""", ("id", messageId));
        await transaction.CommitAsync(Xunit.TestContext.Current.CancellationToken);
        return attached;
    }

    public static async Task DeliverAndProcessAsync(NpgsqlDataSource db, Guid messageId)
    {
        await DeliverToInboxAsync(db, messageId);
        await MarkOutboxDeliveredAsync(db, messageId);
        await ProcessAttachFilesAsync(db, messageId);
    }

    public static async Task WaitForAsync(NpgsqlDataSource db, int processId, string waitEvent)
    {
        for (var i = 0; i < 200; i++)
        {
            var rows = await Db.QueryAsync(db, "SELECT wait_event FROM pg_stat_activity WHERE pid = @pid", ("pid", processId));
            if (rows.Count == 1 && string.Equals(rows[0][0] as string, waitEvent, StringComparison.Ordinal))
            {
                return;
            }

            await Db.ExecAsync(db, "SELECT pg_sleep(0.05)");
        }

        throw new TimeoutException($"Backend {processId} never waited on {waitEvent}.");
    }
}

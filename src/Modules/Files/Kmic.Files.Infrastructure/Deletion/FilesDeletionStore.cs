using Kmic.Files.Domain.Files;
using Kmic.Files.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Infrastructure.Deletion;

/// <summary>
/// 删除队列的读写：领取、完成、失败退避，都是带令牌条件的单条语句，不经聚合、不开事务
/// </summary>
/// <remarks>
/// 队列就是 StoredFile 表里的两类行：状态为“删除中”的，和状态为“已彻底删除”而对象还没删的。
/// 领取用 FOR UPDATE SKIP LOCKED，多个实例同时领取互不重复；结果写入都带“领取令牌仍是本次”的条件，
/// 领取到期被别人接管之后，原来那个实例迟到的结果不会覆盖新的
/// </remarks>
internal sealed class FilesDeletionStore(FilesDbContext context)
{
    private static readonly string _deleting = StoredFileStatus.Deleting.Code;
    private static readonly string _purged = StoredFileStatus.Purged.Code;

    public async Task<IReadOnlyList<DeletionItem>> ClaimAsync(int max, Guid token, TimeSpan lease, int maxAttempts, CancellationToken ct) =>
        await context.Database.SqlQuery<DeletionItem>(
            $"""
            UPDATE "Files"."StoredFile" AS f
            SET "LeaseToken" = {token}, "LeaseUntil" = clock_timestamp() + {lease}, "DeleteAttempts" = f."DeleteAttempts" + 1
            FROM (
                SELECT "Id" FROM "Files"."StoredFile"
                WHERE ("Status" = {_deleting} OR ("Status" = {_purged} AND "ObjectDeletedAt" IS NULL))
                  AND ("NextAttemptAt" IS NULL OR "NextAttemptAt" <= clock_timestamp())
                  AND ("LeaseUntil" IS NULL OR "LeaseUntil" < clock_timestamp())
                  AND "DeleteAttempts" < {maxAttempts}
                ORDER BY COALESCE("NextAttemptAt", "UploadedAt")
                LIMIT {max}
                FOR UPDATE SKIP LOCKED) AS c
            WHERE f."Id" = c."Id"
            RETURNING f."Id" AS "Id", f."StorageKey" AS "StorageKey", f."Status" AS "Status"
            """).ToListAsync(ct);

    // 对象已经删了：删除中的整行删掉（引用随行级联删除），已彻底删除的留下行，记下对象删除的时间
    public async Task CompleteAsync(DeletionItem item, Guid token, CancellationToken ct)
    {
        if (string.Equals(item.Status, _deleting, StringComparison.Ordinal))
        {
            await context.Database.ExecuteSqlAsync(
                $"""DELETE FROM "Files"."StoredFile" WHERE "Id" = {item.Id} AND "LeaseToken" = {token} AND "Status" = {_deleting}""", ct);
            return;
        }

        await context.Database.ExecuteSqlAsync(
            $"""
            UPDATE "Files"."StoredFile"
            SET "ObjectDeletedAt" = clock_timestamp(), "LeaseToken" = NULL, "LeaseUntil" = NULL
            WHERE "Id" = {item.Id} AND "LeaseToken" = {token} AND "Status" = {_purged}
            """, ct);
    }

    // 删除失败：放回队列，按尝试次数退避（1、2、4……分钟，最长 1 小时）
    public async Task FailAsync(DeletionItem item, Guid token, CancellationToken ct) =>
        await context.Database.ExecuteSqlAsync(
            $"""
            UPDATE "Files"."StoredFile"
            SET "LeaseToken" = NULL, "LeaseUntil" = NULL,
                "NextAttemptAt" = clock_timestamp() + LEAST(power(2, "DeleteAttempts" - 1) * interval '1 minute', interval '1 hour')
            WHERE "Id" = {item.Id} AND "LeaseToken" = {token}
            """, ct);
}

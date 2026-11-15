using Kmic.Files.Domain.Files;
using Kmic.Files.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Infrastructure.Deletion;

/// <summary>
/// 文件删除队列存储
/// </summary>
/// <remarks>
/// 领取和结果更新使用单条 SQL，不经聚合方法或显式事务。
/// 队列包含 StoredFile 表中删除中的文件，以及已彻底删除但尚未登记对象删除完成的文件。
/// 通过 FOR UPDATE SKIP LOCKED 避免多实例重复领取；结果更新须匹配本次领取令牌，防止租约到期接管后被旧实例覆盖
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

    // 对象删除后，删除中的文件连同引用记录一起移除；已彻底删除的文件保留记录并登记删除完成
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

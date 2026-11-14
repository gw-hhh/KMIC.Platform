using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 投递队列的读写：领取、记录结果，都是带令牌条件的单条语句，不经聚合、不开事务
/// </summary>
/// <remarks>
/// 队列就是 Delivery 表里待发送的和领取已经到期的行。领取用 FOR UPDATE SKIP LOCKED，多个实例同时领取互不重复，
/// 高优先级的先领；结果写入都带“状态是发送中且领取令牌仍是本次”的条件，领取到期被别人接管之后，
/// 原来那个实例迟到的结果不会覆盖新的。含密模板发完（或最终失败）就把正文和短信参数清空
/// </remarks>
internal sealed class DeliveryStore(NotificationsDbContext context)
{
    private static readonly string _pending = DeliveryStatus.Pending.Code;
    private static readonly string _sending = DeliveryStatus.Sending.Code;
    private static readonly string _sent = DeliveryStatus.Sent.Code;
    private static readonly string _failed = DeliveryStatus.Failed.Code;
    private static readonly string _skipped = DeliveryStatus.Skipped.Code;
    private static readonly string _high = DeliveryPriority.High.Code;

    public async Task<IReadOnlyList<DeliveryItem>> ClaimAsync(int max, Guid token, TimeSpan lease, CancellationToken ct) =>
        await context.Database.SqlQuery<DeliveryItem>(
            $"""
            UPDATE "Notifications"."Delivery" AS d
            SET "Status" = {_sending}, "LeaseToken" = {token}, "LeaseUntil" = clock_timestamp() + {lease}, "Attempts" = d."Attempts" + 1
            FROM (
                SELECT "Id" FROM "Notifications"."Delivery"
                WHERE ("Status" = {_pending} AND ("NextAttemptAt" IS NULL OR "NextAttemptAt" <= clock_timestamp()))
                   OR ("Status" = {_sending} AND "LeaseUntil" < clock_timestamp())
                ORDER BY CASE WHEN "Priority" = {_high} THEN 0 ELSE 1 END, COALESCE("NextAttemptAt", "CreatedAt")
                LIMIT {max}
                FOR UPDATE SKIP LOCKED) AS c
            WHERE d."Id" = c."Id"
            RETURNING d."Id" AS "Id", d."Channel" AS "Channel", d."RecipientUserId" AS "RecipientUserId", d."Address" AS "Address", d."Role" AS "Role",
                      d."Subject" AS "Subject", d."Body" AS "Body", d."SmsParameters"::text AS "SmsParameters", d."AttachmentFileIds"::text AS "AttachmentFileIds", d."TemplateKey" AS "TemplateKey",
                      d."Attempts" AS "Attempts", d."CreatedAt" AS "CreatedAt"
            """).ToListAsync(ct);

    public async Task SentAsync(DeliveryItem item, Guid token, string? providerMessageId, bool clearSecret, CancellationToken ct) =>
        await context.Database.ExecuteSqlAsync(
            $"""
            UPDATE "Notifications"."Delivery"
            SET "Status" = {_sent}, "SentAt" = clock_timestamp(), "ProviderMessageId" = {providerMessageId}, "LastError" = NULL,
                "LeaseToken" = NULL, "LeaseUntil" = NULL,
                "Body" = CASE WHEN {clearSecret} THEN NULL ELSE "Body" END,
                "SmsParameters" = CASE WHEN {clearSecret} THEN NULL ELSE "SmsParameters" END
            WHERE "Id" = {item.Id} AND "LeaseToken" = {token} AND "Status" = {_sending}
            """, ct);

    // 暂时失败：放回队列，按尝试次数退避（30 秒、1 分钟、2 分钟……最长 1 小时）
    public async Task RetryLaterAsync(DeliveryItem item, Guid token, string error, CancellationToken ct) =>
        await context.Database.ExecuteSqlAsync(
            $"""
            UPDATE "Notifications"."Delivery"
            SET "Status" = {_pending}, "LastError" = {Truncate(error)}, "LeaseToken" = NULL, "LeaseUntil" = NULL,
                "NextAttemptAt" = clock_timestamp() + LEAST(power(2, "Attempts" - 1) * interval '30 seconds', interval '1 hour')
            WHERE "Id" = {item.Id} AND "LeaseToken" = {token} AND "Status" = {_sending}
            """, ct);

    public async Task FailedAsync(DeliveryItem item, Guid token, string error, bool clearSecret, CancellationToken ct) =>
        await context.Database.ExecuteSqlAsync(
            $"""
            UPDATE "Notifications"."Delivery"
            SET "Status" = {_failed}, "LastError" = {Truncate(error)}, "LeaseToken" = NULL, "LeaseUntil" = NULL,
                "Body" = CASE WHEN {clearSecret} THEN NULL ELSE "Body" END,
                "SmsParameters" = CASE WHEN {clearSecret} THEN NULL ELSE "SmsParameters" END
            WHERE "Id" = {item.Id} AND "LeaseToken" = {token} AND "Status" = {_sending}
            """, ct);

    // 发送时发现收件人已经没有可用的地址：记为已跳过，原因同处理发送请求时
    public async Task SkippedAsync(DeliveryItem item, Guid token, DeliverySkipReason reason, bool clearSecret, CancellationToken ct)
    {
        var code = reason.Code;
        await context.Database.ExecuteSqlAsync(
            $"""
            UPDATE "Notifications"."Delivery"
            SET "Status" = {_skipped}, "SkipReason" = {code}, "LeaseToken" = NULL, "LeaseUntil" = NULL,
                "Body" = CASE WHEN {clearSecret} THEN NULL ELSE "Body" END,
                "SmsParameters" = CASE WHEN {clearSecret} THEN NULL ELSE "SmsParameters" END
            WHERE "Id" = {item.Id} AND "LeaseToken" = {token} AND "Status" = {_sending}
            """, ct);
    }

    private static string Truncate(string error) => error.Length <= 1000 ? error : error[..1000];
}

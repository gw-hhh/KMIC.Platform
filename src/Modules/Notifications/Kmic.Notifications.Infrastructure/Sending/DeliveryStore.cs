using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 直接读写投递队列，领取与结果更新均使用单条 SQL，不经聚合或显式事务
/// </summary>
/// <remarks>
/// 领取待发送或租约已过期的发送中记录，高优先级优先；通过 FOR UPDATE SKIP LOCKED 避免多实例重复领取。
/// 结果更新须同时匹配发送中状态和本次领取令牌，防止接管后被旧实例覆盖。含密通知发送成功、最终失败或跳过时清空正文及短信参数
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

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    // 投递队列与结果的列不属于聚合，是只由投递工作器和管理用例用带条件的语句更新的影子属性，列名集中定义
    public const string Attempts = DeliveryColumns.Attempts;
    public const string NextAttemptAt = DeliveryColumns.NextAttemptAt;
    public const string LeaseToken = DeliveryColumns.LeaseToken;
    public const string LeaseUntil = DeliveryColumns.LeaseUntil;
    public const string ProviderMessageId = DeliveryColumns.ProviderMessageId;
    public const string LastError = DeliveryColumns.LastError;
    public const string SentAt = DeliveryColumns.SentAt;
    public const string AbandonedAt = DeliveryColumns.AbandonedAt;
    public const string AbandonReason = DeliveryColumns.AbandonReason;

    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable(t => t.HasComment("通知的投递；Notifications 模块；一行是把一条通知用一个渠道发给一个收件人，渲染好的内容随行保存；状态靠带条件的语句更新，不经聚合"));
        builder.Property(d => d.RecipientKey).HasMaxLength(100);
        builder.Property(d => d.AddressMasked).HasMaxLength(200);
        builder.Property(d => d.Address).HasMaxLength(320);
        builder.Property(d => d.Subject).HasMaxLength(500);
        builder.Property(d => d.Body).IsUnboundedText().AuditHidden();
        builder.Property(d => d.SmsParameters).HasColumnType("jsonb").AuditHidden();
        builder.Property(d => d.TemplateKey).HasMaxLength(200);
        builder.Property(d => d.DedupKey).HasMaxLength(300);
        builder.Property(d => d.CodeHash).HasMaxLength(64).AuditHidden();

        builder.Property<int>(Attempts).HasComment("已经尝试发送几次");
        builder.Property<DateTimeOffset?>(NextAttemptAt).HasComment("下次可以领取的时间");
        builder.Property<Guid?>(LeaseToken).HasComment("投递工作器的领取令牌").AuditHidden();
        builder.Property<DateTimeOffset?>(LeaseUntil).HasComment("领取的到期时间");
        builder.Property<string?>(ProviderMessageId).HasMaxLength(200).HasComment("渠道返回的消息标识");
        builder.Property<string?>(LastError).HasMaxLength(1000).HasComment("最近一次失败的原因");
        builder.Property<DateTimeOffset?>(SentAt).HasComment("发送成功的时间");
        builder.Property<DateTimeOffset?>(AbandonedAt).HasComment("放弃处理的时间，没有放弃为空");
        builder.Property<string?>(AbandonReason).HasMaxLength(500).HasComment("放弃处理的原因");

        // 同一个模板、收件人、去重键、渠道只有一条投递：重复的请求或重试处理不会重复发送
        builder.HasIndex(d => new { d.TemplateKey, d.RecipientKey, d.DedupKey, d.Channel }).IsUnique().MapsToError(NotificationsErrors.DeliveryDuplicated);
        builder.HasIndex(d => d.RecipientUserId);
        builder.HasIndex(d => new { d.Status, d.Priority });
        builder.HasIndex(d => d.NotificationId);
        builder.HasNoDataScope("投递记录按权限点管理，不按组织范围；收件人的联系方式已脱敏");
    }
}

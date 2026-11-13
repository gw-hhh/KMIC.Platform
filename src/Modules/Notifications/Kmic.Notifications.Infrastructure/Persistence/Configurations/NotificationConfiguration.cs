using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable(t => t.HasComment("通知发送请求的记录；Notifications 模块；一行是一次发送请求，处理时插入，之后不再修改；来源消息 ID 唯一，保证同一条消息只处理一次"));
        builder.Property(n => n.TemplateKey).HasMaxLength(200);
        builder.Property(n => n.TemplateVersions).HasColumnType("jsonb");
        builder.Property(n => n.DedupKey).HasMaxLength(300);
        builder.Property(n => n.SourceModule).HasMaxLength(50);

        // 同一条来源消息被重复投递时，这里挡住第二次；处理器先查过，撞上唯一索引只可能是两个实例同时处理，重试即可
        builder.HasIndex(n => n.SourceMessageId).IsUnique().MapsToError(NotificationsErrors.DeliveryDuplicated);
        builder.HasNoDataScope("通知记录按权限点管理，不按组织范围");
    }
}

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable(t => t.HasComment("通知模板；Notifications 模块；一行是代码里声明的一种通知，启动时与代码同步；各渠道发布过的内容版本在子表"));
        builder.Property(t => t.Key).HasMaxLength(NotificationTemplate.KeyMaxLength);
        builder.Property(t => t.DisplayName).HasMaxLength(NotificationTemplate.DisplayNameMaxLength);
        builder.Property(t => t.OwnerModule).HasMaxLength(NotificationTemplate.ModuleMaxLength);
        builder.Property(t => t.Channels).HasMaxLength(NotificationTemplate.ChannelsMaxLength);
        builder.Property(t => t.SenderProfile).HasMaxLength(NotificationTemplate.SenderProfileMaxLength);
        builder.Property(t => t.CarriesSecret).AuditHidden();
        builder.Property(t => t.CodeHash).HasMaxLength(64).AuditHidden();
        builder.Property(t => t.ActiveVersions).HasColumnType("jsonb");

        // 两个实例同时启动、同时登记同一个模板时，后一个撞上这里，重试一次就是更新
        builder.HasIndex(t => t.Key).IsUnique().MapsToError(NotificationsErrors.TemplateKeyDuplicated);
        builder.HasMany(t => t.Versions).WithOne().HasForeignKey(v => v.TemplateId);
        builder.HasNoDataScope("模板按权限点管理，不按组织范围");
    }
}

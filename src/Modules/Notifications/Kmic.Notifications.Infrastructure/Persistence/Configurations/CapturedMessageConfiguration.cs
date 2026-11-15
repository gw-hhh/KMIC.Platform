using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Captured;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class CapturedMessageConfiguration : IEntityTypeConfiguration<CapturedMessage>
{
    public void Configure(EntityTypeBuilder<CapturedMessage> builder)
    {
        builder.ToTable(t => t.HasComment("捕获内容（Notifications）：一条记录对应一次邮件或短信投递；开启捕获时保存内容而不实际发送，按保留期清理"));
        builder.Property(c => c.Address).HasMaxLength(320);
        builder.Property(c => c.Subject).HasMaxLength(500);
        builder.Property(c => c.Body).IsUnboundedText();
        builder.HasIndex(c => c.DeliveryId);
        builder.HasIndex(c => c.CapturedAt);
        builder.HasNoDataScope("捕获内容按权限点管理，不按用户或组织划分");
    }
}

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Captured;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class CapturedMessageConfiguration : IEntityTypeConfiguration<CapturedMessage>
{
    public void Configure(EntityTypeBuilder<CapturedMessage> builder)
    {
        builder.ToTable(t => t.HasComment("非生产环境捕获的邮件与短信；Notifications 模块；本该发出去的内容留在这里供查看，只在非生产环境有数据，超过保留期由清理任务删除"));
        builder.Property(c => c.Address).HasMaxLength(320);
        builder.Property(c => c.Subject).HasMaxLength(500);
        builder.Property(c => c.Body).IsUnboundedText();
        builder.HasIndex(c => c.DeliveryId);
        builder.HasIndex(c => c.CapturedAt);
        builder.HasNoDataScope("捕获内容按权限点管理，只在非生产环境存在");
    }
}

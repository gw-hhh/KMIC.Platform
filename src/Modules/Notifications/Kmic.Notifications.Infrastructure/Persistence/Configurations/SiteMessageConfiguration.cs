using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.SiteMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class SiteMessageConfiguration : IEntityTypeConfiguration<SiteMessage>
{
    public void Configure(EntityTypeBuilder<SiteMessage> builder)
    {
        builder.ToTable(t => t.HasComment("站内消息；Notifications 模块；一行是发给某个用户的一条消息，已读是唯一会改的状态；已读和未读超过保留期后由清理任务删除"));
        builder.Property(m => m.Title).HasMaxLength(SiteMessage.TitleMaxLength);
        builder.Property(m => m.Body).HasMaxLength(SiteMessage.BodyMaxLength);
        builder.Property(m => m.Link).HasMaxLength(SiteMessage.LinkMaxLength);
        builder.Property(m => m.Category).HasMaxLength(SiteMessage.CategoryMaxLength);

        // 未读数与列表都按收件人、已读状态、时间走这个索引
        builder.HasIndex(m => new { m.RecipientUserId, m.ReadAt, m.SentAt });
        builder.HasNoDataScope("站内消息按收件人显式过滤，不按组织范围；谁的消息谁看");
    }
}

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class TemplateVersionConfiguration : IEntityTypeConfiguration<TemplateVersion>
{
    public void Configure(EntityTypeBuilder<TemplateVersion> builder)
    {
        builder.ToTable(t => t.HasComment("通知模板的发布版本；Notifications 模块；一行是模板在某个渠道发布过的一版内容，只增不改"));
        builder.Property(v => v.Subject).HasMaxLength(TemplateVersion.SubjectMaxLength);
        builder.Property(v => v.Body).HasMaxLength(TemplateVersion.BodyMaxLength);
        builder.Property(v => v.Remark).HasMaxLength(TemplateVersion.RemarkMaxLength);

        // 同一模板的版本号唯一，跨渠道共用序列
        builder.HasIndex(v => new { v.TemplateId, v.Version }).IsUnique().MapsToError(NotificationsErrors.TemplateVersionDuplicated);
    }
}

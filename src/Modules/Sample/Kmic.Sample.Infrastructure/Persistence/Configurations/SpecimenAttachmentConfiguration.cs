using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Sample.Infrastructure.Persistence.Configurations;

internal sealed class SpecimenAttachmentConfiguration : IEntityTypeConfiguration<SpecimenAttachment>
{
    public void Configure(EntityTypeBuilder<SpecimenAttachment> builder)
    {
        builder.ToTable(t => t.HasComment("示例标本的附件；Sample 模块；一行代表一个附件，保存文件 ID 和关联时的快照，随标本一起保存"));
        builder.Property(a => a.FileName).HasMaxLength(200);
        builder.Property(a => a.FileType).HasMaxLength(50);
        builder.HasIndex("SpecimenId", nameof(SpecimenAttachment.FileId)).IsUnique().MapsToError(SampleErrors.AttachmentExists);
    }
}

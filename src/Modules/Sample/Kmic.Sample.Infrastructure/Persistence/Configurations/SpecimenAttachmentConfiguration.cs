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
        builder.ToTable(t => t.HasComment("示例附件（Sample）：一条记录对应一个附件，保存文件 ID 及关联时的文件信息，随示例一起保存"));
        builder.Property(a => a.FileName).HasMaxLength(200);
        builder.Property(a => a.FileType).HasMaxLength(50);
        builder.HasIndex("SpecimenId", nameof(SpecimenAttachment.FileId)).IsUnique().MapsToError(SampleErrors.AttachmentExists);
    }
}

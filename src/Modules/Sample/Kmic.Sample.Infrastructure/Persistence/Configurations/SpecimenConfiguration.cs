using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Sample.Application;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Sample.Infrastructure.Persistence.Configurations;

internal sealed class SpecimenConfiguration : IEntityTypeConfiguration<Specimen>
{
    public void Configure(EntityTypeBuilder<Specimen> builder)
    {
        builder.ToTable(t => t.HasComment("业务模块示例（Sample）：一条记录对应一个示例；草稿可软删除，提交后可关闭"));
        builder.Property(s => s.Number).HasMaxLength(20).IsCaseInsensitive();
        builder.HasIndex(s => s.Number).IsUnique().MapsToError(SampleErrors.NumberDuplicated);
        builder.Property(s => s.Title).HasMaxLength(100);
        builder.HasMany(s => s.Items).WithOne().HasForeignKey("SpecimenId");
        builder.HasMany(s => s.Attachments).WithOne().HasForeignKey("SpecimenId");
        builder.HasDataScope(SampleScopes.Specimen);
    }
}

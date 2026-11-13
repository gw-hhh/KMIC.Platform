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
        builder.ToTable(t => t.HasComment("示例标本；Sample 模块；一行代表一个标本；可软删除，有状态流转"));
        builder.Property(s => s.Number).HasMaxLength(20).IsCaseInsensitive();
        builder.HasIndex(s => s.Number).IsUnique().MapsToError(SampleErrors.NumberDuplicated);
        builder.Property(s => s.Title).HasMaxLength(100);
        builder.HasMany(s => s.Items).WithOne().HasForeignKey("SpecimenId");
        builder.HasMany(s => s.Attachments).WithOne().HasForeignKey("SpecimenId");
        builder.HasDataScope(SampleScopes.Specimen);
    }
}

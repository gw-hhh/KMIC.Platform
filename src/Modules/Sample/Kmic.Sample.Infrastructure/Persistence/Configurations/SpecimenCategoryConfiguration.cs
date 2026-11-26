using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.SpecimenCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Sample.Infrastructure.Persistence.Configurations;

internal sealed class SpecimenCategoryConfiguration : IEntityTypeConfiguration<SpecimenCategory>
{
    public void Configure(EntityTypeBuilder<SpecimenCategory> builder)
    {
        builder.ToTable(t => t.HasComment("示例分类（Sample）：简单维护档的示例，一行一个分类"));
        builder.Property(c => c.Name).HasMaxLength(50).IsCaseInsensitive();
        builder.HasIndex(c => c.Name).IsUnique().MapsToError(SampleErrors.CategoryNameExists);
        builder.HasNoDataScope("基础资料对所有登录用户一致，按权限点管理");
    }
}

using Kmic.Sample.Domain.Specimens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Sample.Infrastructure.Persistence.Configurations;

internal sealed class SpecimenItemConfiguration : IEntityTypeConfiguration<SpecimenItem>
{
    public void Configure(EntityTypeBuilder<SpecimenItem> builder)
    {
        builder.ToTable(t => t.HasComment("示例明细（Sample）：一条记录对应一项明细，随示例一起保存"));
        builder.Property(i => i.Name).HasMaxLength(50);
    }
}

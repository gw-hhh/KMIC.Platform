using Kmic.Identity.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class CatalogStateRowConfiguration : IEntityTypeConfiguration<CatalogStateRow>
{
    public void Configure(EntityTypeBuilder<CatalogStateRow> builder)
    {
        builder.ToTable("CatalogState", t => t.HasComment("目录同步状态；Identity 模块；上次同步的目录哈希与时间，哈希没变就跳过同步"));
        builder.HasKey(r => r.Name);
        builder.Property(r => r.Name).HasMaxLength(50);
        builder.Property(r => r.ContentHash).HasMaxLength(64);
    }
}

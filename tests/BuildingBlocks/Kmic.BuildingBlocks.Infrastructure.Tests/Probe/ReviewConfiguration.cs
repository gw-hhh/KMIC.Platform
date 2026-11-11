using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable(t => t.HasComment("测试评审；Probe 模块；一行代表一次评审；有状态流转"));
        builder.Property(r => r.Title).HasMaxLength(100);
    }
}

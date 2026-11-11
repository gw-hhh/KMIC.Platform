using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class TicketLineConfiguration : IEntityTypeConfiguration<TicketLine>
{
    public void Configure(EntityTypeBuilder<TicketLine> builder)
    {
        builder.ToTable(t => t.HasComment("测试单据的明细；Probe 模块；一行代表一条明细，随单据保存"));
        builder.Property(l => l.Text).HasMaxLength(50);
    }
}

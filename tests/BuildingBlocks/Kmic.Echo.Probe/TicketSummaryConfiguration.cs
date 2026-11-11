using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Echo.Probe;

internal sealed class TicketSummaryConfiguration : IEntityTypeConfiguration<TicketSummary>
{
    public void Configure(EntityTypeBuilder<TicketSummary> builder)
    {
        builder.ToTable(nameof(TicketSummary), t => t.HasComment("Probe 单据的读模型"));
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.Title).HasMaxLength(200);
    }
}

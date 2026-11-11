using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.Property(t => t.Number).HasMaxLength(20);
        builder.HasIndex(t => t.Number).IsUnique();
        builder.Property(t => t.Title).HasMaxLength(100);
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.Note).IsUnboundedText();
        builder.HasMany(t => t.Lines).WithOne().HasForeignKey("TicketId");
        builder.HasDataScope(ProbeScopes.Ticket);
    }
}

using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable(t => t.HasComment("测试单据；Probe 模块；一行代表一张单据；可软删除"));
        builder.Property(t => t.Number).HasMaxLength(20).IsCaseInsensitive();
        builder.HasIndex(t => t.Number).IsUnique().MapsToError(TicketErrors.NumberDuplicated);
        builder.Property(t => t.ContactEmail).HasMaxLength(256).Classified(KmicData.Personal);
        builder.Property(t => t.AccessTokenHash).HasMaxLength(100).Classified(KmicData.Secret);
        builder.ComplexProperty(t => t.Budget, budget =>
        {
            budget.Property(m => m.Amount).HasPrecision(Precisions.Amount);
            budget.Property(m => m.Currency).HasMaxLength(3);
        });
        builder.ValueObjectCollection(t => t.Watchers);
        builder.Property(t => t.Title).HasMaxLength(100);
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.Note).IsUnboundedText();
        builder.HasMany(t => t.Lines).WithOne().HasForeignKey("TicketId");
        builder.HasDataScope(ProbeScopes.Ticket);
    }
}

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Echo.Probe;

internal sealed class EchoQueryDbContext(DbContextOptions<EchoQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), IEchoQueryDb
{
    public override string Module => "Echo";

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder)
    {
    }

    public IQueryable<TicketSummary> Tickets => Set<TicketSummary>();

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfiguration(new TicketSummaryConfiguration());
}

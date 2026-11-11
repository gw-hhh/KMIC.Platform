using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class ProbeQueryDbContext(DbContextOptions<ProbeQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), IProbeQueryDb
{
    public override string Module => "Probe";

    public IQueryable<Ticket> Tickets => Set<Ticket>();

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInProbeIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfiguration(new TicketConfiguration()).ApplyConfiguration(new TicketLineConfiguration());
}

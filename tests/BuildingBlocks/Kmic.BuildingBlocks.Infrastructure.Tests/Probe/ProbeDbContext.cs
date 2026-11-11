using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class ProbeDbContext(DbContextOptions<ProbeDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    public override string Module => "Probe";

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInProbeIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfiguration(new TicketConfiguration()).ApplyConfiguration(new TicketLineConfiguration()).ApplyConfiguration(new ReviewConfiguration());
}

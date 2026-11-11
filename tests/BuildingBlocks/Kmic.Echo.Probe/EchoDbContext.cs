using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Echo.Probe;

internal sealed class EchoDbContext(DbContextOptions<EchoDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    public override string Module => "Echo";

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder)
    {
    }

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfiguration(new TicketSummaryConfiguration());
}

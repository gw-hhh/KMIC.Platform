using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence.InvalidModel;

internal sealed class InvalidModelDbContext(DbContextOptions<InvalidModelDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    public override string Module => "Probe";

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInProbeIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Gadget>(b =>
        {
            b.ToTable("Gadgets");
            b.HasMany(g => g.Parts).WithOne(p => p.Gadget);
        });
        modelBuilder.Entity<Widget>(b =>
        {
            b.ToTable(nameof(Widget), "Elsewhere");
            b.HasDataScope("Probe.Unknown");
        });
        modelBuilder.Entity<Sprocket>().HasDataScope(OtherScopes.Thing);
    }
}

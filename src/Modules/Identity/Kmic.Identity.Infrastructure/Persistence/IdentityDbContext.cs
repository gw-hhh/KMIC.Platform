using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Persistence;

internal sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    public override string Module => IdentityModuleInfo.Name;

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInIdentityIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) => IdentityModel.Configure(modelBuilder);
}

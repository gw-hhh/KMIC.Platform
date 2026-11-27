using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Infrastructure.Persistence;

internal sealed class ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    public override string Module => ConfigurationModuleInfo.Name;

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInConfigurationIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) => ConfigurationModel.Configure(modelBuilder);
}

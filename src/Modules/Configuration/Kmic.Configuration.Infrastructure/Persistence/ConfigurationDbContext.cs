using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Infrastructure.Persistence;

/// <summary>
/// Configuration 模块的写上下文
/// </summary>
internal sealed class ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    /// <inheritdoc />
    public override string Module => ConfigurationModuleInfo.Name;

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInConfigurationIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => ConfigurationModel.Configure(modelBuilder);
}

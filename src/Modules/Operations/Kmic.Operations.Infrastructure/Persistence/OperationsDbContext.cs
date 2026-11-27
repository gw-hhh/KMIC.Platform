using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Operations.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Operations.Infrastructure.Persistence;

internal sealed class OperationsDbContext(DbContextOptions<OperationsDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    public override string Module => OperationsModuleInfo.Name;

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInOperationsIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) => OperationsModel.Configure(modelBuilder);
}

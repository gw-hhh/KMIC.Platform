using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Sample.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Infrastructure.Persistence;

internal sealed class SampleDbContext(DbContextOptions<SampleDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    public override string Module => SampleModuleInfo.Name;

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInSampleIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) => SampleModel.Configure(modelBuilder);
}

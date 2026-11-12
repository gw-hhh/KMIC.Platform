using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Sample.Contracts;
using Kmic.Sample.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Infrastructure.Persistence;

/// <summary>
/// Sample 模块的写入上下文：命令里通过仓储使用
/// </summary>
internal sealed class SampleDbContext(DbContextOptions<SampleDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    /// <inheritdoc />
    public override string Module => SampleModuleInfo.Name;

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInSampleIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfiguration(new SpecimenConfiguration()).ApplyConfiguration(new SpecimenItemConfiguration());
}

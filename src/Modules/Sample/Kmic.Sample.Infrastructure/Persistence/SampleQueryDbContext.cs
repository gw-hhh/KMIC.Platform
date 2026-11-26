using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Sample.Application;
using Kmic.Sample.Contracts;
using Kmic.Sample.Domain.SpecimenCategories;
using Kmic.Sample.Domain.Specimens;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Infrastructure.Persistence;

/// <summary>
/// Sample 模块的查询上下文，不跟踪实体
/// </summary>
internal sealed class SampleQueryDbContext(DbContextOptions<SampleQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), ISampleQueryDb
{
    /// <inheritdoc />
    public override string Module => SampleModuleInfo.Name;

    /// <inheritdoc />
    public IQueryable<Specimen> Specimens => Set<Specimen>();

    /// <inheritdoc />
    public IQueryable<SpecimenCategory> SpecimenCategories => Set<SpecimenCategory>();

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInSampleIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => SampleModel.Configure(modelBuilder);
}

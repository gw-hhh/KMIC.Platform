using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Sample.Application;
using Kmic.Sample.Contracts;
using Kmic.Sample.Domain.SpecimenCategories;
using Kmic.Sample.Domain.Specimens;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Infrastructure.Persistence;

internal sealed class SampleQueryDbContext(DbContextOptions<SampleQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), ISampleQueryDb
{
    public override string Module => SampleModuleInfo.Name;

    public IQueryable<Specimen> Specimens => Set<Specimen>();

    public IQueryable<SpecimenCategory> SpecimenCategories => Set<SpecimenCategory>();
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInSampleIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) => SampleModel.Configure(modelBuilder);
}

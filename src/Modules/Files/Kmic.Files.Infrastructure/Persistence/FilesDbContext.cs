using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Infrastructure.Persistence;

internal sealed class FilesDbContext(DbContextOptions<FilesDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    public override string Module => FilesModuleInfo.Name;

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInFilesIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) => FilesModel.Configure(modelBuilder);
}

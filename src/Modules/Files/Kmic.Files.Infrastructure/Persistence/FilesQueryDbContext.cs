using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Application;
using Kmic.Files.Contracts;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Infrastructure.Persistence;

internal sealed class FilesQueryDbContext(DbContextOptions<FilesQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), IFilesQueryDb
{
    public override string Module => FilesModuleInfo.Name;

    public IQueryable<StoredFile> Files => Set<StoredFile>();

    public IQueryable<StorageIssue> StorageIssues => Set<StorageIssue>();

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInFilesIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) => FilesModel.Configure(modelBuilder);
}

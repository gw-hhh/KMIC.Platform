using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Infrastructure.Persistence;

/// <summary>
/// Files 模块的写入上下文：命令里通过仓储使用
/// </summary>
internal sealed class FilesDbContext(DbContextOptions<FilesDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    /// <inheritdoc />
    public override string Module => FilesModuleInfo.Name;

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInFilesIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => FilesModel.Configure(modelBuilder);
}

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Application;
using Kmic.Files.Contracts;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Infrastructure.Persistence;

/// <summary>
/// Files 模块的查询上下文：不跟踪实体，查询处理器通过 IFilesQueryDb 使用
/// </summary>
internal sealed class FilesQueryDbContext(DbContextOptions<FilesQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), IFilesQueryDb
{
    /// <inheritdoc />
    public override string Module => FilesModuleInfo.Name;

    /// <inheritdoc />
    public IQueryable<StoredFile> Files => Set<StoredFile>();

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInFilesIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => FilesModel.Configure(modelBuilder);
}

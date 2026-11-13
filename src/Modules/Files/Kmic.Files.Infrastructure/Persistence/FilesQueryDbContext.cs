using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Application;
using Kmic.Files.Contracts;
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
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // 与 FilesDbContext 一致
    }
}

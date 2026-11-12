using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Persistence;

/// <summary>
/// Identity 模块的写入上下文：命令里通过仓储使用
/// </summary>
internal sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    /// <inheritdoc />
    public override string Module => IdentityModuleInfo.Name;

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInIdentityIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => IdentityModel.Configure(modelBuilder);
}

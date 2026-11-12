using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Operations.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Operations.Infrastructure.Persistence;

/// <summary>
/// Operations 模块的写入上下文：命令里通过仓储使用
/// </summary>
internal sealed class OperationsDbContext(DbContextOptions<OperationsDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    /// <inheritdoc />
    public override string Module => OperationsModuleInfo.Name;

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInOperationsIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => OperationsModel.Configure(modelBuilder);
}

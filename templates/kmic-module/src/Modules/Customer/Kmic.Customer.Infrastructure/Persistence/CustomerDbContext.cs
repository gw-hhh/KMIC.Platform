using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Customer.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Customer.Infrastructure.Persistence;

/// <summary>
/// Customer 模块的写入上下文：命令里通过仓储使用
/// </summary>
internal sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    /// <inheritdoc />
    public override string Module => CustomerModuleInfo.Name;

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // 有强类型 ID 之后，在 CustomerIdConverters 上标 [EfCoreConverter<XxxId>]，并在这里调用 RegisterAllInCustomerIdConverters
    }
}

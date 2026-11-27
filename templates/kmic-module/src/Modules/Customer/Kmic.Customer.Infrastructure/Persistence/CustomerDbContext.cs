using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Customer.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Customer.Infrastructure.Persistence;

// 在命令处理器中通过仓储使用
internal sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    public override string Module => CustomerModuleInfo.Name;

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // 定义强类型 ID 后，在 CustomerIdConverters 标注 [EfCoreConverter<XxxId>] 并在此注册
    }
}

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Customer.Application;
using Kmic.Customer.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Customer.Infrastructure.Persistence;

// 只读查询上下文，禁用实体跟踪
internal sealed class CustomerQueryDbContext(DbContextOptions<CustomerQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), ICustomerQueryDb
{
    public override string Module => CustomerModuleInfo.Name;

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // 复用与写入上下文一致的强类型 ID 值转换器
    }
}

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Customer.Application;
using Kmic.Customer.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Customer.Infrastructure.Persistence;

/// <summary>
/// Customer 模块的查询上下文：不跟踪实体，查询处理器通过 ICustomerQueryDb 使用
/// </summary>
internal sealed class CustomerQueryDbContext(DbContextOptions<CustomerQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), ICustomerQueryDb
{
    /// <inheritdoc />
    public override string Module => CustomerModuleInfo.Name;

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // 与 CustomerDbContext 一致
    }
}

using System.Reflection;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Customer.Application;
using Kmic.Customer.Contracts;
using Kmic.Customer.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.Customer.Infrastructure;

/// <summary>注册 Customer 模块私有服务（处理器、校验器与 EF 配置会自动发现）</summary>
public sealed class CustomerModule : IModule
{
    public string Name => CustomerModuleInfo.Name;

    public Assembly ApplicationAssembly => typeof(ICustomerQueryDb).Assembly;

    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModuleDbContexts<CustomerDbContext, CustomerQueryDbContext, ICustomerQueryDb>();
    }
}

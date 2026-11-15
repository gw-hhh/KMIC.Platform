using System.Reflection;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Customer.Application;
using Kmic.Customer.Contracts;
using Kmic.Customer.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.Customer.Infrastructure;

/// <summary>
/// Customer 模块的服务端注册类
/// </summary>
/// <remarks>
/// 处理器、校验器和 EF 配置自动发现，此处补充模块服务注册
/// </remarks>
public sealed class CustomerModule : IModule
{
    /// <inheritdoc />
    public string Name => CustomerModuleInfo.Name;

    /// <inheritdoc />
    public Assembly ApplicationAssembly => typeof(ICustomerQueryDb).Assembly;

    /// <inheritdoc />
    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModuleDbContexts<CustomerDbContext, CustomerQueryDbContext, ICustomerQueryDb>();
    }
}

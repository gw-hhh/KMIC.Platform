using Microsoft.Extensions.DependencyInjection;

namespace Spike01.Customers;

/// <summary>
/// 客户模块的服务注册（原型中代替 IModule）
/// </summary>
public static class CustomersServiceCollectionExtensions
{
    public static IServiceCollection AddCustomersModule(this IServiceCollection services) =>
        services.AddScoped<ICustomerSelectionQueries, CustomerSelectionQueries>();
}

using Kmic.ModuleName.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.ModuleName.Infrastructure;

/// <summary>
/// ModuleName 模块的服务注册
/// </summary>
public static class ModuleNameModule
{
    public static IServiceCollection AddModuleNameModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ModuleNameDbContext>(options => options.UseNpgsql(connectionString));
        services.AddDbContext<ModuleNameQueryDbContext>(options =>
            options.UseNpgsql(connectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
        return services;
    }
}

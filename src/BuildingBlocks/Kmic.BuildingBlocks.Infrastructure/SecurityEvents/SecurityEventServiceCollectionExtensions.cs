using Kmic.BuildingBlocks.Application.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

/// <summary>
/// 安全事件的服务注册
/// </summary>
public static class SecurityEventServiceCollectionExtensions
{
    /// <summary>
    /// 注册安全事件的队列、写入器和后台写库，Api、Worker 两种角色都要调用
    /// </summary>
    /// <remarks>
    /// 写入器用 TryAdd，测试先注册了记录用的写入器就不会被覆盖。事件在哪个进程里产生，就在哪个进程里写库
    /// </remarks>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddKmicSecurityEvents(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<SecurityEventOptions>();
        services.TryAddSingleton<SecurityEventQueue>();
        services.TryAddScoped<ISecurityEventWriter, SecurityEventWriter>();
        services.AddHostedService<SecurityEventFlusher>();
        return services;
    }
}

using Kmic.BuildingBlocks.Application.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

public static class SecurityEventServiceCollectionExtensions
{
    /// <summary>注册安全事件队列、写入器及后台刷盘服务（Api 与 Worker 均需调用）</summary>
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

using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 消息与缓存失效的监听连接，断开算降级
/// </summary>
internal sealed class ListenerDependencyCheck(MessagingListener listener) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!listener.IsRunning)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("监听线程没有运行"));
        }

        return Task.FromResult(listener.IsConnected
            ? HealthCheckResult.Healthy("监听连接正常")
            : HealthCheckResult.Degraded("监听连接已断开，正在重连"));
    }
}

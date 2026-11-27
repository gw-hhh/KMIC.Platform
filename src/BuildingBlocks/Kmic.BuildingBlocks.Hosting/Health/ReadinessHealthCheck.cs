using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Kmic.BuildingBlocks.Hosting.Health;

internal sealed class ReadinessHealthCheck(StartupState startup, IHostApplicationLifetime lifetime, DatabaseProbe database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (lifetime.ApplicationStopping.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("正在停机");
        }

        if (!startup.IsCompleted)
        {
            return HealthCheckResult.Unhealthy("启动任务尚未完成");
        }

        try
        {
            await database.PingAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is NpgsqlException or OperationCanceledException or TimeoutException)
        {
            return HealthCheckResult.Unhealthy("数据库不可用", exception);
        }
    }
}

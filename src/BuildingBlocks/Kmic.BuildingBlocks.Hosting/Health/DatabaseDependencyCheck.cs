using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 数据库
/// </summary>
internal sealed class DatabaseDependencyCheck(DatabaseProbe database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await database.PingAsync(cancellationToken);
            return HealthCheckResult.Healthy("可以连接");
        }
        catch (Exception exception) when (exception is NpgsqlException or OperationCanceledException or TimeoutException)
        {
            return HealthCheckResult.Unhealthy("连接失败或超时", exception);
        }
    }
}

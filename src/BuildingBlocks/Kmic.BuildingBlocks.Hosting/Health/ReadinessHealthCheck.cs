using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 就绪检查：没有它整个服务就无法工作的条件
/// </summary>
/// <remarks>
/// 只检查三件事：数据库可连接、启动任务已完成、不在停机中。对象存储、短信网关这类局部依赖故障时，
/// 实例仍然接收请求，其余功能正常，所以不放在这里，见依赖检查
/// </remarks>
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
            return HealthCheckResult.Unhealthy("启动任务还没有完成");
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

using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 就绪检查：没有它整个服务就无法工作的条件
/// </summary>
/// <remarks>
/// 只检查三件事：数据库可连接、启动任务已完成、不在停机中。对象存储、短信网关这类局部依赖故障时，
/// 实例仍然接收请求，其余功能正常，所以不放在这里。检查本身有超时，不会拖住编排平台的探测
/// </remarks>
internal sealed class ReadinessHealthCheck(StartupState startup, IHostApplicationLifetime lifetime, IOptions<DatabaseOptions> database) : IHealthCheck, IDisposable
{
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(3);

    private readonly Lazy<NpgsqlDataSource> _dataSource = new(() => NpgsqlDataSource.Create(
        new NpgsqlConnectionStringBuilder(database.Value.ConnectionString) { MaxPoolSize = 2, Pooling = true, ApplicationName = "kmic-health" }.ConnectionString));

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

        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limit.CancelAfter(_timeout);
        try
        {
            await using var connection = await _dataSource.Value.OpenConnectionAsync(limit.Token);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(limit.Token);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is NpgsqlException or OperationCanceledException or TimeoutException)
        {
            return HealthCheckResult.Unhealthy("数据库不可用", exception);
        }
    }

    public void Dispose()
    {
        if (_dataSource.IsValueCreated)
        {
            _dataSource.Value.Dispose();
        }
    }
}

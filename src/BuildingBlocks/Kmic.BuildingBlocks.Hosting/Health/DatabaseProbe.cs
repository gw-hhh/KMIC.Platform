using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 健康检查用的数据库探测，就绪检查和依赖检查共用
/// </summary>
/// <remarks>
/// 自己有一个很小的连接池，不占业务的连接池：业务池被占满时，探测仍然能区分“数据库挂了”和“业务把池用光了”
/// </remarks>
internal sealed class DatabaseProbe(IOptions<DatabaseOptions> database) : IDisposable
{
    /// <summary>
    /// 一次探测最多等多久，编排平台的探测不能被拖住
    /// </summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

    private readonly Lazy<NpgsqlDataSource> _dataSource = new(() => NpgsqlDataSource.Create(
        new NpgsqlConnectionStringBuilder(database.Value.ConnectionString) { MaxPoolSize = 2, Pooling = true, ApplicationName = "kmic-health" }.ConnectionString));

    /// <summary>
    /// 执行一条最简单的查询，连不上、超时都会抛出异常
    /// </summary>
    public async Task PingAsync(CancellationToken ct)
    {
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limit.CancelAfter(Timeout);
        await using var connection = await _dataSource.Value.OpenConnectionAsync(limit.Token);
        await using var command = new NpgsqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync(limit.Token);
    }

    public void Dispose()
    {
        if (_dataSource.IsValueCreated)
        {
            _dataSource.Value.Dispose();
        }
    }
}

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Hosting.Health;

internal sealed class DatabaseProbe(IOptions<DatabaseOptions> database) : IDisposable
{
    // 限制单次探测超时，避免阻塞编排平台探针
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

    private readonly Lazy<NpgsqlDataSource> _dataSource = new(() => NpgsqlDataSource.Create(
        new NpgsqlConnectionStringBuilder(database.Value.ConnectionString) { MaxPoolSize = 2, Pooling = true, ApplicationName = "kmic-health" }.ConnectionString));

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

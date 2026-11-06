using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Spike15.Tests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);
        await using var source = Pool("kmic-setup", maxPoolSize: 1);
        await using var context = new ItemsContext(source, readOnly: false);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    // 每个数据源一个独立的连接池；Timeout 是池满时借连接的最长等待（秒）
    public NpgsqlDataSource Pool(string applicationName, int maxPoolSize, int timeoutSeconds = 3)
    {
        var builder = new NpgsqlDataSourceBuilder(_container.GetConnectionString());
        builder.ConnectionStringBuilder.ApplicationName = applicationName;
        builder.ConnectionStringBuilder.MaxPoolSize = maxPoolSize;
        builder.ConnectionStringBuilder.Timeout = timeoutSeconds;
        return builder.Build();
    }
}

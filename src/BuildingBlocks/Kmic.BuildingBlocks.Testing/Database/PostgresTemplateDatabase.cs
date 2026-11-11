using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Kmic.BuildingBlocks.Testing.Database;

/// <summary>
/// 模板数据库
/// </summary>
/// <remarks>
/// 程序集级夹具：启动 PostgreSQL 容器并建好模板库。测试项目派生一个类实现 InitializeTemplateAsync，用 [assembly: AssemblyFixture(typeof(派生类))] 注册，
/// 测试类再通过 IClassFixture&lt;ClassDatabase&lt;派生类&gt;&gt; 拿到克隆库
/// </remarks>
public abstract class PostgresTemplateDatabase : IAsyncLifetime
{
    /// <summary>
    /// 和生产环境的 PostgreSQL 小版本保持一致
    /// </summary>
    public const string Image = "postgres:18.6";

    private const string TemplateName = "kmic_template";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(Image).Build();

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        await _container.StartAsync(ct);
        await ExecuteAsync($"CREATE DATABASE {TemplateName}", ct);

        // 不用连接池：池里空闲的连接也会让克隆失败
        await InitializeTemplateAsync(ConnectionString(TemplateName, pooling: false), ct);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 克隆数据库，返回库名
    /// </summary>
    public async Task<string> CloneAsync(CancellationToken ct)
    {
        var database = $"test_{Guid.CreateVersion7():N}";
        await ExecuteAsync($"CREATE DATABASE \"{database}\" TEMPLATE {TemplateName}", ct);
        return database;
    }

    /// <summary>
    /// 删除克隆库，有连接时强制断开
    /// </summary>
    public Task DropAsync(string database, CancellationToken ct) =>
        ExecuteAsync($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)", ct);

    /// <summary>
    /// 连接串
    /// </summary>
    public string ConnectionString(string database, bool pooling = true) =>
        new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = database, Pooling = pooling }.ConnectionString;

    /// <summary>
    /// 建立模板库的结构，如执行迁移
    /// </summary>
    /// <remarks>
    /// 连接串不用连接池，不要另外建池化的连接
    /// </remarks>
    protected abstract Task InitializeTemplateAsync(string connectionString, CancellationToken ct);

    private async Task ExecuteAsync(string sql, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(ConnectionString("postgres", pooling: false));
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(ct);
    }
}

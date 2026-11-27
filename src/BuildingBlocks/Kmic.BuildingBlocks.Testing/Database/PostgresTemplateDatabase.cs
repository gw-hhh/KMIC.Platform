using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Kmic.BuildingBlocks.Testing.Database;

/// <summary>PostgreSQL 模板数据库夹具（程序集级），派生类通过 [assembly: AssemblyFixture(typeof(派生类))] 注册。</summary>
public abstract class PostgresTemplateDatabase : IAsyncLifetime
{
    /// <summary>与生产环境的 PostgreSQL 次版本号保持一致。</summary>
    public const string Image = "postgres:18.6";

    private const string TemplateName = "kmic_template";

    // 测试类并行执行时，每个含 Worker 角色的宿主会同时开启前台与后台连接池，默认 100 连接上限易耗尽
    // （表现为个别测试随机报“剩余连接槽位留给超级用户”）；此处放宽测试库连接上限，生产环境按运维预算公式配置
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(Image)
        .WithName(TestContainerNames.For("postgres"))
        .WithLabel(TestContainerNames.GroupLabel, TestContainerNames.Group)
        .WithPortBinding(TestPorts.Free(), PostgreSqlBuilder.PostgreSqlPort)
        .WithCommand("-c", "max_connections=500")
        .Build();

    public async ValueTask InitializeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        await _container.StartAsync(ct);
        await ExecuteAsync($"CREATE DATABASE {TemplateName}", ct);

        // 禁用连接池：池中残留空闲连接会导致数据库克隆失败
        await InitializeTemplateAsync(ConnectionString(TemplateName, pooling: false), ct);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>从模板库克隆新数据库并返回库名。</summary>
    public async Task<string> CloneAsync(CancellationToken ct)
    {
        var database = $"test_{Guid.CreateVersion7():N}";
        await ExecuteAsync($"CREATE DATABASE \"{database}\" TEMPLATE {TemplateName}", ct);
        return database;
    }

    /// <summary>创建空数据库并返回库名，用于迁移逻辑测试。</summary>
    public async Task<string> CreateEmptyAsync(CancellationToken ct)
    {
        var database = $"test_{Guid.CreateVersion7():N}";
        await ExecuteAsync($"CREATE DATABASE \"{database}\"", ct);
        return database;
    }

    /// <summary>删除指定克隆库（存在活跃连接时强制断开）。</summary>
    public Task DropAsync(string database, CancellationToken ct) =>
        ExecuteAsync($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)", ct);

    /// <summary>获取指定数据库的连接字符串。</summary>
    public string ConnectionString(string database, bool pooling = true) =>
        new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = database, Pooling = pooling }.ConnectionString;

    /// <summary>初始化模板库结构（如执行迁移）；传入的连接已禁用连接池，请勿额外创建池化连接。</summary>
    protected abstract Task InitializeTemplateAsync(string connectionString, CancellationToken ct);

    private async Task ExecuteAsync(string sql, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(ConnectionString("postgres", pooling: false));
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(ct);
    }
}

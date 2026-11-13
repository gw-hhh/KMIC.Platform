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

    // 测试类并行运行，每个带 Worker 角色的宿主要开前台、后台各两个连接池；默认的 100 个连接会被用光，
    // 表现是个别测试随机报“剩余连接槽位留给超级用户”。测试库的上限放宽，生产的连接预算按运维手册的公式另算
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(Image)
        .WithName(TestContainerNames.For("postgres"))
        .WithLabel(TestContainerNames.GroupLabel, TestContainerNames.Group)
        .WithPortBinding(TestPorts.Free(), PostgreSqlBuilder.PostgreSqlPort)
        .WithCommand("-c", "max_connections=500")
        .Build();

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
    /// 建一个没有任何结构的空库，返回库名
    /// </summary>
    /// <remarks>
    /// 用来测试迁移本身。角色属于整个实例，空库里同样能用
    /// </remarks>
    public async Task<string> CreateEmptyAsync(CancellationToken ct)
    {
        var database = $"test_{Guid.CreateVersion7():N}";
        await ExecuteAsync($"CREATE DATABASE \"{database}\"", ct);
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

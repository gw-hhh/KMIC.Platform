using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Kmic.BuildingBlocks.Testing.Database;

/// <summary>
/// 程序集级夹具：启动 PostgreSQL 容器并生成模板库，测试类从它克隆自己的数据库（06 D8）
/// </summary>
/// <remarks>
/// 集成测试项目派生一个类实现 InitializeTemplateAsync，并用 [assembly: AssemblyFixture(typeof(派生类))] 注册；
/// 测试类通过 IClassFixture&lt;ClassDatabase&lt;派生类&gt;&gt; 取得克隆库
/// </remarks>
public abstract class PostgresTemplateDatabase : IAsyncLifetime
{
    /// <summary>
    /// 镜像版本与生产的 PostgreSQL 小版本一致（S0-04）
    /// </summary>
    public const string Image = "postgres:18.6";

    private const string TemplateName = "kmic_template";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(Image).Build();

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await _container.StartAsync(cancellationToken);
        await ExecuteAsync($"CREATE DATABASE {TemplateName}", cancellationToken);

        // 不进连接池，连接关闭即断开：池中空闲的物理连接同样会让克隆失败（55006，原型 #10）
        await InitializeTemplateAsync(ConnectionString(TemplateName, pooling: false), cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 从模板库克隆一个新数据库，返回库名
    /// </summary>
    public async Task<string> CloneAsync(CancellationToken cancellationToken)
    {
        var database = $"test_{Guid.CreateVersion7():N}";
        await ExecuteAsync($"CREATE DATABASE \"{database}\" TEMPLATE {TemplateName}", cancellationToken);
        return database;
    }

    /// <summary>
    /// 删除克隆库，仍有连接时强制断开
    /// </summary>
    public Task DropAsync(string database, CancellationToken cancellationToken) =>
        ExecuteAsync($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)", cancellationToken);

    /// <summary>
    /// 某个数据库的连接串
    /// </summary>
    public string ConnectionString(string database, bool pooling = true) =>
        new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = database, Pooling = pooling }.ConnectionString;

    /// <summary>
    /// 在模板库上建立结构（执行迁移等）；连接串不进连接池，不要另外建立池化的连接
    /// </summary>
    protected abstract Task InitializeTemplateAsync(string connectionString, CancellationToken cancellationToken);

    private async Task ExecuteAsync(string sql, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(ConnectionString("postgres", pooling: false));
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Spike05.Tests;

// 原型 #13 要在阿里云 RDS PG 18 上运行：设置环境变量 KMIC_SPIKE13_CONNECTION（迁移账号的连接串）即连到该实例；
// 不设置时启动本机 PG 18 容器作对照。所有对象建在一个随机命名的 schema 中，结束时删除，不在目标库留下痕迹
public sealed class TargetDatabase : IAsyncLifetime
{
    public const string ConnectionVariable = "KMIC_SPIKE13_CONNECTION";

    private PostgreSqlContainer? _container;

    public NpgsqlDataSource Source { get; private set; } = null!;

    public string ConnectionString { get; private set; } = null!;

    public string Schema { get; } = $"spike13_{Guid.CreateVersion7():N}"[..24];

    public bool IsExternal { get; private set; }

    public bool TextSchemaCreated { get; set; }

    private bool _createdSharedCollation;

    public async ValueTask InitializeAsync()
    {
        var external = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (string.IsNullOrWhiteSpace(external))
        {
            _container = new PostgreSqlBuilder("postgres:18").Build();
            await _container.StartAsync(TestContext.Current.CancellationToken);
            ConnectionString = _container.GetConnectionString();
        }
        else
        {
            IsExternal = true;
            ConnectionString = external;
        }

        Source = NpgsqlDataSource.Create(ConnectionString);
        await Db.ExecAsync(Source, $"""CREATE SCHEMA "{Schema}" """);

        // 迁移程序的框架步骤：在任何模块迁移之前创建共用的排序规则
        _createdSharedCollation = (await Db.QueryAsync(Source, "SELECT 1 FROM pg_collation WHERE collname = 'CaseInsensitive' AND collnamespace = 'public'::regnamespace")).Count == 0;
        await Db.ExecAsync(Source, """CREATE COLLATION IF NOT EXISTS public."CaseInsensitive" (provider = icu, locale = 'und-u-ks-level2', deterministic = false)""");
    }

    public async ValueTask DisposeAsync()
    {
        await Db.ExecAsync(Source, $"""DROP SCHEMA IF EXISTS "{Schema}" CASCADE""");
        if (_createdSharedCollation)
        {
            await Db.ExecAsync(Source, """DROP COLLATION IF EXISTS public."CaseInsensitive" """);
        }

        await Source.DisposeAsync();
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}

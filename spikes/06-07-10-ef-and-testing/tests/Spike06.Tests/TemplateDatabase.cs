using System.Diagnostics;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Spike06.Tests;

// 程序集级夹具：启动容器、执行“迁移”生成模板库；测试类从模板库克隆自己的数据库（06 D8）
public sealed class TemplateDatabase : IAsyncLifetime
{
    public const string TemplateName = "kmic_template";

    // 本原型的“迁移”：发布 1 之后的表结构（新旧两列并存）与按天统计用的事件表
    private const string Migrations = """
        CREATE SCHEMA app;
        CREATE TABLE app."Sample" ("Id" uuid PRIMARY KEY, "Name" text NULL, "DisplayName" text NULL);
        CREATE TABLE app."Event" ("Id" uuid PRIMARY KEY, "OccurredAt" timestamptz NOT NULL);
        """;

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public static int Instances { get; private set; }

    public TimeSpan MigrationTime { get; private set; }

    public async ValueTask InitializeAsync()
    {
        Instances++;
        await _container.StartAsync(TestContext.Current.CancellationToken);
        await using (var admin = AdminSource())
        {
            await Db.ExecAsync(admin, $"CREATE DATABASE {TemplateName}");
        }

        var started = Stopwatch.GetTimestamp();

        // 迁移连接不进连接池：用完即断开，模板库上不留活动连接（克隆的前提）
        await using (var template = NpgsqlDataSource.Create(ConnectionString(TemplateName, pooling: false)))
        {
            await Db.ExecAsync(template, Migrations);
        }

        MigrationTime = Stopwatch.GetElapsedTime(started);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public async Task<string> CloneAsync(string strategy = "WAL_LOG")
    {
        var name = $"test_{Guid.CreateVersion7():N}";
        await using var admin = AdminSource();
        await Db.ExecAsync(admin, $"CREATE DATABASE {name} TEMPLATE {TemplateName} STRATEGY {strategy}");
        return ConnectionString(name, pooling: true);
    }

    public NpgsqlDataSource AdminSource() => NpgsqlDataSource.Create(ConnectionString("postgres", pooling: false));

    public string ConnectionString(string database, bool pooling) =>
        new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = database, Pooling = pooling }.ConnectionString;
}

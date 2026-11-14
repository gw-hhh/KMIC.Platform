using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.SecurityEvents;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Echo.Probe;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 本测试项目的模板库：Probe、Echo 两个模块和生产一样通过迁移建立结构，另建一个运行账号供权限测试用
/// </summary>
public sealed class ProbeTemplate : PostgresTemplateDatabase
{
    /// <summary>
    /// 运行账号的登录名
    /// </summary>
    /// <remarks>
    /// 角色属于整个 PostgreSQL 实例，克隆库里同样能用
    /// </remarks>
    public const string RuntimeLogin = "kmic_app";

    // 每次启动容器时随机生成，不写进代码
    public string RuntimePassword { get; } = Guid.CreateVersion7().ToString("N");

    // 安全事件表属于 Identity 模块，框架测试里没有这个模块，建 schema 并像模块初始迁移那样授权给运行账号
    private static string IdentitySchemaSql =>
        $"""
        CREATE SCHEMA "Identity";
        GRANT USAGE ON SCHEMA "Identity" TO {MigrationBuilderExtensions.RuntimeRole};
        ALTER DEFAULT PRIVILEGES IN SCHEMA "Identity" GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {MigrationBuilderExtensions.RuntimeRole};
        """;

    /// <summary>
    /// 运行账号的连接串
    /// </summary>
    public string RuntimeConnectionString(string database) =>
        new NpgsqlConnectionStringBuilder(ConnectionString(database, pooling: false)) { Username = RuntimeLogin, Password = RuntimePassword }.ConnectionString;

    protected override async Task InitializeTemplateAsync(string connectionString, CancellationToken ct)
    {
        await using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync(ct);
            await using var roles = connection.CreateCommand();
            roles.CommandText = $"""
                CREATE ROLE {MigrationBuilderExtensions.RuntimeRole} NOLOGIN;
                CREATE ROLE {RuntimeLogin} LOGIN PASSWORD '{RuntimePassword}' IN ROLE {MigrationBuilderExtensions.RuntimeRole};
                """;
            await roles.ExecuteNonQueryAsync(ct);
        }

        await ModuleMigrator.PrepareDatabaseAsync(connectionString, ct);
        await ModuleMigrator.MigrateAsync<ProbeDbContext>(connectionString, ct);
        await ModuleMigrator.MigrateAsync<EchoDbContext>(connectionString, ct);

        // 路由表、Quartz 表和定时任务的两张表属于 Operations 模块，框架测试里没有这个模块，直接建表
        await using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync(ct);
            await using var routes = connection.CreateCommand();
            routes.CommandText = """CREATE SCHEMA "Operations";""" + MessageRouteTable.CreateSql + MessagingInstanceTable.CreateSql + SchedulingTables.CreateSql + SchedulingTables.QuartzCreateSql() + IdentitySchemaSql + SecurityEventTable.CreateSql;
            await routes.ExecuteNonQueryAsync(ct);
        }
    }
}

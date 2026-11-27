using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.SecurityEvents;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Echo.Probe;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>Probe 与 Echo 模块测试模板库，另建运行账号供权限测试使用</summary>
public sealed class ProbeTemplate : PostgresTemplateDatabase
{
    /// <summary>运行账号登录名（实例级角色，克隆库通用）</summary>
    public const string RuntimeLogin = "kmic_app";

    // 每次启动容器时随机生成，不写进代码
    public string RuntimePassword { get; } = Guid.CreateVersion7().ToString("N");

    // 安全事件表归属 Identity 模块，此处手动创建对应 Schema 并按模块迁移规范授予运行账号权限
    private static string IdentitySchemaSql =>
        $"""
        CREATE SCHEMA "Identity";
        GRANT USAGE ON SCHEMA "Identity" TO {MigrationBuilderExtensions.RuntimeRole};
        ALTER DEFAULT PRIVILEGES IN SCHEMA "Identity" GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {MigrationBuilderExtensions.RuntimeRole};
        """;

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

        // 路由表、Quartz 表及定时任务表归属 Operations 模块，此处直接初始化对应表结构
        await using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync(ct);
            await using var routes = connection.CreateCommand();
            routes.CommandText = """CREATE SCHEMA "Operations";""" + MessageRouteTable.CreateSql + MessagingInstanceTable.CreateSql + AdmissionTable.CreateSql + SchedulingTables.CreateSql + SchedulingTables.QuartzCreateSql() + IdentitySchemaSql + SecurityEventTable.CreateSql;
            await routes.ExecuteNonQueryAsync(ct);
        }
    }
}

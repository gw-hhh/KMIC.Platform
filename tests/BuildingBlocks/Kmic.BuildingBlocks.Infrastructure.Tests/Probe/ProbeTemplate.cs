using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Testing.Database;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 本测试程序集的模板库：与生产一样以迁移建立结构，另建运行账号（组角色的成员）供权限测试使用
/// </summary>
public sealed class ProbeTemplate : PostgresTemplateDatabase
{
    /// <summary>
    /// 运行账号的登录名；角色属于整个 PostgreSQL 实例，克隆库同样可用
    /// </summary>
    public const string RuntimeLogin = "kmic_app";

    // 每次启动容器随机生成，不写进代码
    public string RuntimePassword { get; } = Guid.CreateVersion7().ToString("N");

    /// <summary>
    /// 以运行账号连接某个库的连接串
    /// </summary>
    public string RuntimeConnectionString(string database) =>
        new NpgsqlConnectionStringBuilder(ConnectionString(database, pooling: false)) { Username = RuntimeLogin, Password = RuntimePassword }.ConnectionString;

    protected override async Task InitializeTemplateAsync(string connectionString, CancellationToken cancellationToken)
    {
        await using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var roles = connection.CreateCommand();
            roles.CommandText = $"""
                CREATE ROLE {MigrationBuilderExtensions.RuntimeRole} NOLOGIN;
                CREATE ROLE {RuntimeLogin} LOGIN PASSWORD '{RuntimePassword}' IN ROLE {MigrationBuilderExtensions.RuntimeRole};
                """;
            await roles.ExecuteNonQueryAsync(cancellationToken);
        }

        await ModuleMigrator.PrepareDatabaseAsync(connectionString, cancellationToken);
        await ModuleMigrator.MigrateAsync<ProbeDbContext>(connectionString, cancellationToken);
    }
}

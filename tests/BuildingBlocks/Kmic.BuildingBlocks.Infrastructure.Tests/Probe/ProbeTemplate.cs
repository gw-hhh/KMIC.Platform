using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Testing.Database;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 本测试项目的模板库：和生产一样通过迁移建立结构，另建一个运行账号供权限测试用
/// </summary>
public sealed class ProbeTemplate : PostgresTemplateDatabase
{
    /// <summary>
    /// 运行账号的登录名；角色属于整个 PostgreSQL 实例，克隆库里同样能用
    /// </summary>
    public const string RuntimeLogin = "kmic_app";

    // 每次启动容器时随机生成，不写进代码
    public string RuntimePassword { get; } = Guid.CreateVersion7().ToString("N");

    /// <summary>
    /// 运行账号的连接串
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

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Configuration.Infrastructure;
using Kmic.Identity.Infrastructure;
using Kmic.Operations.Infrastructure;
using Kmic.Sample.Infrastructure;
using Npgsql;

namespace Kmic.Api.Tests;

/// <summary>
/// API 测试的模板库：各模块和生产一样通过迁移建立结构，另建一个运行账号，宿主用它连接
/// </summary>
public sealed class ApiTemplate : PostgresTemplateDatabase
{
    /// <summary>
    /// 运行账号的登录名
    /// </summary>
    public const string RuntimeLogin = "kmic_app";

    /// <summary>
    /// 运行账号的密码，每次启动容器时随机生成，不写进代码
    /// </summary>
    public string RuntimePassword { get; } = Guid.CreateVersion7().ToString("N");

    /// <summary>
    /// 运行账号的连接串
    /// </summary>
    public string RuntimeConnectionString(string database) =>
        new NpgsqlConnectionStringBuilder(ConnectionString(database, pooling: true)) { Username = RuntimeLogin, Password = RuntimePassword }.ConnectionString;

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

        // 每个模块的 AddServices 登记了它的 DbContext，迁移不用认识具体类型；Identity 与 Operations 的框架表由它们自己的迁移建
        var modules = new ServiceCollection();
        new ConfigurationModule().AddServices(modules);
        new IdentityModule().AddServices(modules);
        new OperationsModule().AddServices(modules);
        new SampleModule().AddServices(modules);
        await using var provider = modules.BuildServiceProvider();
        await ModuleMigrator.MigrateAllAsync(provider, connectionString, ct);
    }
}

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Configuration.Infrastructure;
using Kmic.Files.Infrastructure;
using Kmic.Identity.Infrastructure;
using Kmic.Notifications.Infrastructure;
using Kmic.Operations.Infrastructure;
using Kmic.Sample.Infrastructure;
using Npgsql;

namespace Kmic.Api.Tests;

/// <summary>API 测试模板库：各模块通过迁移建表，并单独创建运行账号供宿主连接</summary>
public sealed class ApiTemplate : PostgresTemplateDatabase
{
    public const string RuntimeLogin = "kmic_app";

    public string RuntimePassword { get; } = Guid.CreateVersion7().ToString("N");

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

        // 各模块的 AddServices 已注册其 DbContext，迁移过程无需感知具体类型；Identity 与 Operations 的框架表由各自迁移创建
        var modules = new ServiceCollection();
        new ConfigurationModule().AddServices(modules);
        new FilesModule().AddServices(modules);
        new IdentityModule().AddServices(modules);
        new NotificationsModule().AddServices(modules);
        new OperationsModule().AddServices(modules);
        new SampleModule().AddServices(modules);
        await using var provider = modules.BuildServiceProvider();
        await ModuleMigrator.MigrateAllAsync(provider, connectionString, ct);
    }
}

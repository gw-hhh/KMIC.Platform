using System.Reflection;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Testing.Database;
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
        foreach (var module in HostModules())
        {
            module.AddServices(modules);
        }

        await using var provider = modules.BuildServiceProvider();
        await ModuleMigrator.MigrateAllAsync(provider, connectionString, ct);
    }

    // 宿主引用的全部模块：新建模块的脚本把它加进宿主后，这里自动包含，不用另外登记
    private static IEnumerable<IModule> HostModules() =>
        typeof(Program).Assembly.GetReferencedAssemblies()
            .Select(Assembly.Load)
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(IModule)))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .Select(t => (IModule)Activator.CreateInstance(t)!);
}

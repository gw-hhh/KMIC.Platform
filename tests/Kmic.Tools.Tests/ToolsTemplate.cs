using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Testing.Database;
using Npgsql;

namespace Kmic.Tools.Tests;

/// <summary>
/// 命令行程序测试的模板库：只有运行账号的角色，结构由被测的迁移程序自己建立
/// </summary>
public sealed class ToolsTemplate : PostgresTemplateDatabase
{
    protected override async Task InitializeTemplateAsync(string connectionString, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var role = connection.CreateCommand();
        role.CommandText = $"CREATE ROLE {MigrationBuilderExtensions.RuntimeRole} NOLOGIN;";
        await role.ExecuteNonQueryAsync(ct);
    }
}

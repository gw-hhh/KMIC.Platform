using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Testing.Database;
using Npgsql;

namespace Kmic.Tools.Tests;

/// <summary>仅预置运行账号角色，表结构由被测迁移程序自行建立</summary>
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

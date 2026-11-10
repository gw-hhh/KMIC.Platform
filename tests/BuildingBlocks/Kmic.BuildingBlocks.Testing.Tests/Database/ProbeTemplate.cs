using Kmic.BuildingBlocks.Testing.Database;
using Npgsql;

namespace Kmic.BuildingBlocks.Testing.Tests.Database;

/// <summary>
/// 本测试程序集的模板库：只有一张探测表，代替真实迁移
/// </summary>
public sealed class ProbeTemplate : PostgresTemplateDatabase
{
    protected override async Task InitializeTemplateAsync(string connectionString, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""CREATE TABLE "Probe" ("Id" uuid PRIMARY KEY, "Owner" text NOT NULL)""", connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

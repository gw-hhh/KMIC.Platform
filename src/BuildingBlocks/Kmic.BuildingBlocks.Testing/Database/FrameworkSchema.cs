using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.SecurityEvents;
using Npgsql;

namespace Kmic.BuildingBlocks.Testing.Database;

/// <summary>
/// 测试模板库里的框架表
/// </summary>
public static class FrameworkSchema
{
    /// <summary>
    /// 建 Operations、Identity 两个 schema 里框架要用的表
    /// </summary>
    public static async Task CreateAsync(string connectionString, CancellationToken ct, bool identityHasMigrations = false, bool operationsHasMigrations = false)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        var identity = identityHasMigrations
            ? string.Empty
            : $"""
                CREATE SCHEMA "Identity";
                GRANT USAGE ON SCHEMA "Identity" TO {MigrationBuilderExtensions.RuntimeRole};
                ALTER DEFAULT PRIVILEGES IN SCHEMA "Identity" GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {MigrationBuilderExtensions.RuntimeRole};
                """;
        var operations = operationsHasMigrations
            ? string.Empty
            : $"""
                CREATE SCHEMA "Operations";
                GRANT USAGE ON SCHEMA "Operations" TO {MigrationBuilderExtensions.RuntimeRole};
                ALTER DEFAULT PRIVILEGES IN SCHEMA "Operations" GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {MigrationBuilderExtensions.RuntimeRole};
                """ + MessageRouteTable.CreateSql + MessagingInstanceTable.CreateSql + AdmissionTable.CreateSql + SchedulingTables.CreateSql + SchedulingTables.QuartzCreateSql();
        command.CommandText = operations + identity + (identityHasMigrations ? string.Empty : SecurityEventTable.CreateSql);
        await command.ExecuteNonQueryAsync(ct);
    }
}

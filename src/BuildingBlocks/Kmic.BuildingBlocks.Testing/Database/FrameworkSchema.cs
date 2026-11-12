using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.SecurityEvents;
using Npgsql;

namespace Kmic.BuildingBlocks.Testing.Database;

/// <summary>
/// 测试模板库里的框架表
/// </summary>
/// <remarks>
/// 消息路由表、调度表属于 Operations 模块，安全事件表属于 Identity 模块。这两个模块建好之前，
/// 框架测试和 API 测试直接在模板库里建这些表，并像模块的初始迁移那样授权给运行账号
/// </remarks>
public static class FrameworkSchema
{
    /// <summary>
    /// 建 Operations、Identity 两个 schema 里框架要用的表
    /// </summary>
    public static async Task CreateAsync(string connectionString, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE SCHEMA "Operations";
            CREATE SCHEMA "Identity";
            GRANT USAGE ON SCHEMA "Operations" TO {MigrationBuilderExtensions.RuntimeRole};
            ALTER DEFAULT PRIVILEGES IN SCHEMA "Operations" GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {MigrationBuilderExtensions.RuntimeRole};
            GRANT USAGE ON SCHEMA "Identity" TO {MigrationBuilderExtensions.RuntimeRole};
            ALTER DEFAULT PRIVILEGES IN SCHEMA "Identity" GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {MigrationBuilderExtensions.RuntimeRole};
            """ + MessageRouteTable.CreateSql + SchedulingTables.CreateSql + SchedulingTables.QuartzCreateSql() + SecurityEventTable.CreateSql;
        await command.ExecuteNonQueryAsync(ct);
    }
}

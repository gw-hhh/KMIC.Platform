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
/// 消息路由表、调度表属于 Operations 模块，安全事件表属于 Identity 模块。没有接这两个模块迁移的测试（框架测试）
/// 直接在模板库里建这些表，并像模块的初始迁移那样授权给运行账号。
/// 已经接了模块迁移的测试宿主，调用时传 identityHasMigrations、operationsHasMigrations 跳过对应的那一部分
/// </remarks>
public static class FrameworkSchema
{
    /// <summary>
    /// 建 Operations、Identity 两个 schema 里框架要用的表
    /// </summary>
    /// <param name="connectionString">模板库的连接串</param>
    /// <param name="ct">取消令牌</param>
    /// <param name="identityHasMigrations">Identity 模块的迁移会建自己的 schema 与安全事件表时为真，这里就不再建</param>
    /// <param name="operationsHasMigrations">Operations 模块的迁移会建自己的 schema、消息路由表、调度表与 Quartz 表时为真，这里就不再建</param>
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
                """ + MessageRouteTable.CreateSql + MessagingInstanceTable.CreateSql + SchedulingTables.CreateSql + SchedulingTables.QuartzCreateSql();
        command.CommandText = operations + identity + (identityHasMigrations ? string.Empty : SecurityEventTable.CreateSql);
        await command.ExecuteNonQueryAsync(ct);
    }
}

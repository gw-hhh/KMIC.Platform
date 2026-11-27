using Microsoft.EntityFrameworkCore.Migrations;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

public static class MigrationBuilderExtensions
{
    /// <summary>运行账号组角色名，实际登录账号为其成员</summary>
    public const string RuntimeRole = "kmic_runtime";

    /// <summary>创建模块 schema 并授权运行账号（须置于初始迁移最前部）</summary>
    public static void InitializeModuleSchema(this MigrationBuilder migrationBuilder, string schema)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        migrationBuilder.Sql($"""CREATE SCHEMA IF NOT EXISTS "{schema}";""");
        migrationBuilder.Sql($"""GRANT USAGE ON SCHEMA "{schema}" TO {RuntimeRole};""");
        // 迁移历史表在首个迁移执行前即已创建，不受默认权限规则覆盖，需显式授权运行账号读取以支持启动迁移检查
        migrationBuilder.Sql($"""GRANT SELECT ON "{schema}"."__EFMigrationsHistory" TO {RuntimeRole};""");
        migrationBuilder.Sql($"""ALTER DEFAULT PRIVILEGES IN SCHEMA "{schema}" GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {RuntimeRole};""");
    }
}

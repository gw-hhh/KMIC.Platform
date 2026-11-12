using Microsoft.EntityFrameworkCore.Migrations;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 迁移扩展
/// </summary>
public static class MigrationBuilderExtensions
{
    /// <summary>
    /// 运行账号的组角色
    /// </summary>
    /// <remarks>
    /// 实际登录的账号是它的成员，迁移脚本里就不会出现和环境有关的账号名
    /// </remarks>
    public const string RuntimeRole = "kmic_runtime";

    /// <summary>
    /// 建立模块 schema 并授权给运行账号
    /// </summary>
    /// <remarks>
    /// 写在模块初始迁移的最前面。之后建的表运行账号自动可读写，但不能改表结构；迁移历史表只可读
    /// </remarks>
    /// <param name="migrationBuilder">迁移</param>
    /// <param name="schema">模块名</param>
    public static void InitializeModuleSchema(this MigrationBuilder migrationBuilder, string schema)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        migrationBuilder.Sql($"""CREATE SCHEMA IF NOT EXISTS "{schema}";""");
        migrationBuilder.Sql($"""GRANT USAGE ON SCHEMA "{schema}" TO {RuntimeRole};""");
        // 迁移历史表在执行第一个迁移之前就建好了，默认授权管不到它；启动时的迁移检查用运行账号读它
        migrationBuilder.Sql($"""GRANT SELECT ON "{schema}"."__EFMigrationsHistory" TO {RuntimeRole};""");
        migrationBuilder.Sql($"""ALTER DEFAULT PRIVILEGES IN SCHEMA "{schema}" GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {RuntimeRole};""");
    }
}

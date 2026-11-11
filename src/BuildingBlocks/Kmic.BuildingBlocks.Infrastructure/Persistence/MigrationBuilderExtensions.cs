using Microsoft.EntityFrameworkCore.Migrations;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模块迁移中使用的框架步骤
/// </summary>
public static class MigrationBuilderExtensions
{
    /// <summary>
    /// 运行账号的组角色；实际登录的运行账号是它的成员，迁移脚本中不出现环境相关的账号名
    /// </summary>
    public const string RuntimeRole = "kmic_runtime";

    /// <summary>
    /// 建立模块 schema 并授权给运行账号：之后新建的表自动可读写，但运行账号不能执行 DDL；写在模块初始迁移的最前面
    /// </summary>
    /// <param name="migrationBuilder">迁移</param>
    /// <param name="schema">模块名，也是 schema 名</param>
    public static void InitializeModuleSchema(this MigrationBuilder migrationBuilder, string schema)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        migrationBuilder.Sql($"""CREATE SCHEMA IF NOT EXISTS "{schema}";""");
        migrationBuilder.Sql($"""GRANT USAGE ON SCHEMA "{schema}" TO {RuntimeRole};""");
        migrationBuilder.Sql($"""ALTER DEFAULT PRIVILEGES IN SCHEMA "{schema}" GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {RuntimeRole};""");
    }
}

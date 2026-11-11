using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace Kmic.BuildingBlocks.Infrastructure.History;

/// <summary>
/// 建只追加的表之后，收回运行账号的修改和删除权限
/// </summary>
/// <remarks>
/// 表默认会把增删改查都授权给运行账号，所以要在建表后立刻撤销。过期数据由迁移账号清理
/// </remarks>
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage", Justification = "Npgsql 的 SQL 生成器构造函数要这个内部类型，替换它没有公开的写法")]
internal sealed class AppendOnlyMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, INpgsqlSingletonOptions options)
    : NpgsqlMigrationsSqlGenerator(dependencies, options)
{
    protected override void Generate(CreateTableOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate = true)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(builder);

        base.Generate(operation, model, builder, terminate);
        if (!terminate || !IsAppendOnly(operation, model))
        {
            return;
        }

        builder
            .Append("REVOKE UPDATE, DELETE ON TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
            .Append(" FROM ")
            .Append(MigrationBuilderExtensions.RuntimeRole)
            .AppendLine(";")
            .EndCommand();
    }

    private static bool IsAppendOnly(CreateTableOperation operation, IModel? model) =>
        operation.FindAnnotation(StateHistory.AppendOnlyAnnotation)?.Value is true
        || model?.GetRelationalModel().FindTable(operation.Name, operation.Schema)?.EntityTypeMappings
            .Any(m => m.TypeBase.FindAnnotation(StateHistory.AppendOnlyAnnotation)?.Value is true) == true;
}

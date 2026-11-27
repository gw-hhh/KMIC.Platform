using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace Kmic.BuildingBlocks.Infrastructure.History;

// 只追加表建表后回收运行账号的 UPDATE/DELETE 权限
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage", Justification = "Npgsql 的 SQL 生成器构造函数依赖此内部类型，暂无公开替代方式")]
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

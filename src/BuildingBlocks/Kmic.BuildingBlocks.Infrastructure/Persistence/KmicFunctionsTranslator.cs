using System.Reflection;
using Kmic.BuildingBlocks.Application.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal sealed class KmicFunctionsTranslator(NpgsqlSqlExpressionFactory sql) : IMethodCallTranslator
{
    private static readonly MethodInfo _containsText = typeof(KmicFunctions).GetMethod(nameof(KmicFunctions.ContainsText))!;
    private static readonly MethodInfo _startsWithText = typeof(KmicFunctions).GetMethod(nameof(KmicFunctions.StartsWithText))!;

    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method != _containsText && method != _startsWithText)
        {
            return null;
        }

        // 显式转换为 "C" 排序规则：不区分大小写列采用非确定性排序规则，PostgreSQL 不支持对其直接执行 ILIKE
        var column = new CollateExpression(arguments[0], "C");
        var term = sql.ApplyTypeMapping(arguments[1], column.TypeMapping);

        // 通配符转义在 SQL 侧通过 replace 函数完成，确保检索词始终作为参数化查询传入
        var escaped = Replace(Replace(Replace(term, @"\", @"\\"), "%", @"\%"), "_", @"\_");
        var anything = sql.Constant("%", term.TypeMapping);
        var pattern = method == _containsText ? sql.Add(sql.Add(anything, escaped), anything) : sql.Add(escaped, anything);
        return sql.ILike(column, pattern, sql.Constant(@"\", term.TypeMapping));
    }

    private SqlExpression Replace(SqlExpression value, string from, string to) =>
        sql.Function(
            "replace",
            [value, sql.Constant(from, value.TypeMapping), sql.Constant(to, value.TypeMapping)],
            nullable: true,
            argumentsPropagateNullability: [true, true, true],
            typeof(string),
            value.TypeMapping);
}

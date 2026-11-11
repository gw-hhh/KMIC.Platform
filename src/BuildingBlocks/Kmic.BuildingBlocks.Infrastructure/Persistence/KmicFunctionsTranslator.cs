using System.Reflection;
using Kmic.BuildingBlocks.Application.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// KmicFunctions 翻译器
/// </summary>
/// <remarks>
/// 翻译为 ("列" COLLATE "C") ILIKE 模式 ESCAPE '\'
/// </remarks>
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

        // 显式转成 "C"：不区分大小写的列用的是非确定性排序规则，不支持 ILIKE
        var column = new CollateExpression(arguments[0], "C");
        var term = sql.ApplyTypeMapping(arguments[1], column.TypeMapping);

        // 转义在 SQL 里用 replace 做，term 始终作为参数传入
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

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Spike05.Tests;

// 翻译为 ("Col" COLLATE "C") ILIKE ('%' || 转义(term) || '%') ESCAPE '\'。
// 转义在数据库端做（replace），term 始终是参数；显式的 "C" 让默认列与非确定性排序规则列写法相同
internal sealed class KmicFunctionsTranslator(NpgsqlSqlExpressionFactory sql) : IMethodCallTranslator
{
    private static readonly MethodInfo _containsText = typeof(KmicFunctions).GetMethod(nameof(KmicFunctions.ContainsText))!;
    private static readonly MethodInfo _startsWithText = typeof(KmicFunctions).GetMethod(nameof(KmicFunctions.StartsWithText))!;

    public SqlExpression? Translate(
        SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method != _containsText && method != _startsWithText)
        {
            return null;
        }

        var column = new CollateExpression(arguments[0], "C");
        var term = sql.ApplyTypeMapping(arguments[1], column.TypeMapping);
        var escaped = Replace(Replace(Replace(term, @"\", @"\\"), "%", @"\%"), "_", @"\_");
        var pattern = method == _containsText
            ? sql.Add(sql.Add(sql.Constant("%", term.TypeMapping), escaped), sql.Constant("%", term.TypeMapping))
            : sql.Add(escaped, sql.Constant("%", term.TypeMapping));

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

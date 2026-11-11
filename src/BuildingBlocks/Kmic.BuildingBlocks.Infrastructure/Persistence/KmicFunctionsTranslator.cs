using System.Reflection;
using Kmic.BuildingBlocks.Application.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 把 KmicFunctions 翻译为 ("列" COLLATE "C") ILIKE 模式 ESCAPE '\'
/// </summary>
/// <remarks>
/// 转义在数据库端完成（replace），term 始终是参数，翻译器拿不到它的值；
/// 显式的 "C" 让默认列与不区分大小写的列写法相同（非确定性排序规则不支持 ILIKE），ILIKE 在 "C" 下只折叠 ASCII 字母
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

        var column = new CollateExpression(arguments[0], "C");
        var term = sql.ApplyTypeMapping(arguments[1], column.TypeMapping);
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

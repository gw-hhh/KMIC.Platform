using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Spike06.Tests;

// 翻译为 CAST("Col" AT TIME ZONE '业务时区' AS date)。业务时区是部署配置，进程内不变，作为常量写入 SQL
internal sealed class BusinessDateTranslator(ISqlExpressionFactory sql, IRelationalTypeMappingSource typeMappings, string timeZone) : IMethodCallTranslator
{
    private static readonly MethodInfo _toBusinessDate = typeof(KmicFunctions).GetMethod(nameof(KmicFunctions.ToBusinessDate))!;

    public SqlExpression? Translate(
        SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method != _toBusinessDate)
        {
            return null;
        }

        var local = new AtTimeZoneExpression(
            arguments[0],
            sql.Constant(timeZone, typeMappings.FindMapping(typeof(string))),
            typeof(DateTime),
            typeMappings.FindMapping(typeof(DateTime), "timestamp without time zone"));
        return sql.Convert(local, typeof(DateOnly), typeMappings.FindMapping(typeof(DateOnly)));
    }
}

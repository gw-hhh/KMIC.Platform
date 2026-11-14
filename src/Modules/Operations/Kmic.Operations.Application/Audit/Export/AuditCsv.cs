using System.Buffers;
using System.Globalization;
using System.Text;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 审计导出的 CSV 写法
/// </summary>
internal static class AuditCsv
{
    private static readonly SearchValues<char> _special = SearchValues.Create(",\"\r\n");

    /// <summary>
    /// 表头，列的顺序和 <see cref="Line" /> 一致
    /// </summary>
    public const string Header = "发生时间,模块,动作,来源,操作人编号,操作人,实际操作人编号,实际操作人,追踪编号,操作对象";

    /// <summary>
    /// 一行：每个单元格按需加引号，Excel 会把 = + - @ 开头的单元格当成公式，前面补一个单引号
    /// </summary>
    public static string Line(
        DateTimeOffset occurredAt,
        string module,
        string action,
        string origin,
        Guid? actorUserId,
        string? actorName,
        Guid? actualUserId,
        string? actualName,
        string traceId,
        string targets) =>
        string.Join(
            ',',
            Cell(occurredAt.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture)),
            Cell(module),
            Cell(action),
            Cell(origin),
            Cell(actorUserId?.ToString()),
            Cell(actorName),
            Cell(actualUserId?.ToString()),
            Cell(actualName),
            Cell(traceId),
            Cell(targets));

    private static string Cell(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var text = value[0] is '=' or '+' or '-' or '@' or '\t' or '\r' ? "'" + value : value;
        if (!text.AsSpan().ContainsAny(_special))
        {
            return text;
        }

        return new StringBuilder(text.Length + 2).Append('"').Append(text.Replace("\"", "\"\"", StringComparison.Ordinal)).Append('"').ToString();
    }
}

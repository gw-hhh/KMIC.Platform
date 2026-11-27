using System.Buffers;
using System.Globalization;
using System.Text;

namespace Kmic.Operations.Application.Audit.Export;

internal static class AuditCsv
{
    private static readonly SearchValues<char> _special = SearchValues.Create(",\"\r\n");

    // 列顺序须与 Line 保持一致
    public const string Header = "发生时间,模块,动作,来源,操作人编号,操作人,实际操作人编号,实际操作人,追踪编号,操作对象";

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

        // 公式前缀及制表符、回车开头的内容加单引号，避免被电子表格当作公式
        var text = value[0] is '=' or '+' or '-' or '@' or '\t' or '\r' ? "'" + value : value;
        if (!text.AsSpan().ContainsAny(_special))
        {
            return text;
        }

        return new StringBuilder(text.Length + 2).Append('"').Append(text.Replace("\"", "\"\"", StringComparison.Ordinal)).Append('"').ToString();
    }
}

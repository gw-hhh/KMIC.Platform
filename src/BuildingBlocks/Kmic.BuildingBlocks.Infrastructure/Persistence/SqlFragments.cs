using System.Runtime.CompilerServices;
using System.Text;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 把几段带参数的 SQL 拼成一条
/// </summary>
/// <remarks>
/// 插值字符串里再嵌插值字符串会变成一个参数值，要用它拼
/// </remarks>
public static class SqlFragments
{
    /// <summary>
    /// 按顺序拼接，各段的占位符编号顺延，参数按原样保留
    /// </summary>
    public static FormattableString Concat(params FormattableString[] parts)
    {
        ArgumentNullException.ThrowIfNull(parts);

        var format = new StringBuilder();
        var arguments = new List<object?>();
        foreach (var part in parts)
        {
            if (part is null)
            {
                throw new ArgumentException("拼接的各段不能为空。", nameof(parts));
            }

            AppendRenumbered(format, part.Format, arguments.Count);
            arguments.AddRange(part.GetArguments());
        }

        return FormattableStringFactory.Create(format.ToString(), [.. arguments]);
    }

    // 逐字符扫描：{{ 和 }} 是转义的花括号，原样保留；{n、{n,对齐、{n:格式} 里的 n 加上偏移
    private static void AppendRenumbered(StringBuilder target, string format, int offset)
    {
        var i = 0;
        while (i < format.Length)
        {
            var c = format[i];
            if ((c == '{' || c == '}') && i + 1 < format.Length && format[i + 1] == c)
            {
                target.Append(c).Append(c);
                i += 2;
            }
            else if (c == '{')
            {
                var start = i + 1;
                var end = start;
                while (end < format.Length && char.IsAsciiDigit(format[end]))
                {
                    end++;
                }

                if (end == start)
                {
                    throw new FormatException($"SQL 片段里的占位符不对：{format}");
                }

                target.Append('{').Append(int.Parse(format.AsSpan(start, end - start), System.Globalization.CultureInfo.InvariantCulture) + offset);
                i = end;
            }
            else
            {
                target.Append(c);
                i++;
            }
        }
    }
}

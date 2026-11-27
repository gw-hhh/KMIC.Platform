using System.Runtime.CompilerServices;
using System.Text;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>参数化 SQL 片段拼接工具（防止 FormattableString 直接嵌套时内层被整体当成单个参数）</summary>
public static class SqlFragments
{
    /// <summary>按顺序拼接多个 SQL 片段，自动顺延参数占位符编号并保留参数值</summary>
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

    // 逐字符扫描格式化模板：转义花括号 {{ 与 }} 原样保留，占位符 {n}、{n,对齐}、{n:格式} 中的索引 n 按当前参数偏移量递增
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
                    throw new FormatException($"SQL 片段中的占位符格式无效：{format}");
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

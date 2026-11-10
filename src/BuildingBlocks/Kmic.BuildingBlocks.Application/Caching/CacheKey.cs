using System.Globalization;
using System.Text;

namespace Kmic.BuildingBlocks.Application.Caching;

/// <summary>
/// 缓存键与标签的统一格式“模块:对象:标识…”，版本号放在最后；只在模块的 XxxCacheKeys 静态类中调用
/// </summary>
public static class CacheKey
{
    private const char Separator = ':';

    /// <summary>
    /// 生成键或标签
    /// </summary>
    /// <param name="module">模块名；框架自己的键用 "Kmic"</param>
    /// <param name="item">对象，如 "RoleSnapshot"</param>
    /// <param name="identifiers">标识，按不变区域格式化</param>
    public static string Of(string module, string item, params ReadOnlySpan<object> identifiers)
    {
        var key = new StringBuilder();
        Append(key, module, nameof(module));
        Append(key, item, nameof(item));
        foreach (var identifier in identifiers)
        {
            var text = identifier is IFormattable formattable
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : identifier?.ToString();
            Append(key, text, nameof(identifiers));
        }

        return key.ToString();
    }

    private static void Append(StringBuilder key, string? segment, string parameterName)
    {
        // 段内出现分隔符会让两个不同的键拼成同一个字符串
        if (string.IsNullOrWhiteSpace(segment) || segment.Contains(Separator, StringComparison.Ordinal))
        {
            throw new ArgumentException($"缓存键的段不能为空，也不能包含“{Separator}”：“{segment}”。", parameterName);
        }

        if (key.Length > 0)
        {
            key.Append(Separator);
        }

        key.Append(segment);
    }
}

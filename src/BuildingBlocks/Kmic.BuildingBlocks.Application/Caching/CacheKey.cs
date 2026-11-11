using System.Globalization;
using System.Text;

namespace Kmic.BuildingBlocks.Application.Caching;

/// <summary>
/// 缓存键
/// </summary>
/// <remarks>
/// 格式为“模块:对象:标识”，只在各模块的 XxxCacheKeys 类里调用
/// </remarks>
public static class CacheKey
{
    private const char Separator = ':';

    /// <summary>
    /// 生成缓存键或标签
    /// </summary>
    /// <param name="module">模块名，框架自己用 "Kmic"</param>
    /// <param name="item">对象，如 "RoleSnapshot"</param>
    /// <param name="identifiers">标识</param>
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
        // 段里不能出现分隔符，否则两个不同的键可能拼成同一个字符串
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

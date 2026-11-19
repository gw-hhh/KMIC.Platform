using System.Globalization;
using System.Text;

namespace Kmic.BuildingBlocks.Application.Caching;

/// <summary>
/// 缓存键，格式是“模块:对象:标识”
/// </summary>
/// <remarks>
/// 只在各模块的 XxxCacheKeys 类里调用
/// </remarks>
public static class CacheKey
{
    private const char Separator = ':';

    /// <summary>
    /// 生成缓存键或标签
    /// </summary>
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

    /// <summary>
    /// 缓存键的类别，即前两段“模块:对象”
    /// </summary>
    /// <remarks>
    /// 标识放在第三段以后，否则指标维度会失控
    /// </remarks>
    public static string CategoryOf(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        var first = key.IndexOf(Separator, StringComparison.Ordinal);
        var second = first < 0 ? -1 : key.IndexOf(Separator, first + 1);
        return second < 0 ? key : key[..second];
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

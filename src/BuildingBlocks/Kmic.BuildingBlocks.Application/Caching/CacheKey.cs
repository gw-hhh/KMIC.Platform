using System.Globalization;
using System.Text;

namespace Kmic.BuildingBlocks.Application.Caching;

/// <summary>缓存键工具，格式为“模块:对象:标识”，仅限在各模块的 XxxCacheKeys 类中调用。</summary>
public static class CacheKey
{
    private const char Separator = ':';

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

    /// <summary>提取前两段“模块:对象”作为指标分类，避免标识段导致维度基数失控。</summary>
    public static string CategoryOf(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        var first = key.IndexOf(Separator, StringComparison.Ordinal);
        var second = first < 0 ? -1 : key.IndexOf(Separator, first + 1);
        return second < 0 ? key : key[..second];
    }

    private static void Append(StringBuilder key, string? segment, string parameterName)
    {
        // 各段禁止包含分隔符，避免不同缓存键拼接后产生冲突
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

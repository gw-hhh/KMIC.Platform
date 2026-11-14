using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Application.Settings;

namespace Kmic.Configuration.Infrastructure.Settings;

/// <summary>
/// 已保存系统设置值的缓存快照
/// </summary>
/// <remarks>
/// JSON 值在构造时固定，解析结果按类型缓存并复用，调用方须保持返回值不可变。
/// 并发首次读取可能重复解析，类型缓存仅在内存中使用，不参与 JSON 序列化
/// </remarks>
[ImmutableObject(true)]
internal sealed class SettingsSnapshot(IReadOnlyDictionary<string, string> values)
{
    private readonly ConcurrentDictionary<Type, object> _typed = new();

    /// <summary>
    /// 设置键到 JSON 值
    /// </summary>
    public IReadOnlyDictionary<string, string> Values { get; } = values;

    /// <summary>
    /// 读取指定设置，未保存、JSON 解析失败或解析结果为 null 时使用代码默认值
    /// </summary>
    public T Get<T>(string key)
        where T : class, ISystemSetting<T> =>
        (T)_typed.GetOrAdd(typeof(T), _ => Read<T>(key));

    private T Read<T>(string key)
        where T : class, ISystemSetting<T>
    {
        if (!Values.TryGetValue(key, out var json))
        {
            return T.Default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, KmicJsonOptions.Stored) ?? T.Default;
        }
        catch (JsonException)
        {
            return T.Default;
        }
    }

    [JsonIgnore]
    internal int TypedCount => _typed.Count;
}

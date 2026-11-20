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
/// 调用方不能修改返回值
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

    /// <summary>
    /// 把保存的 JSON 读成设置值，解析失败或结果为 null 时使用代码默认值
    /// </summary>
    public static T ParseOrDefault<T>(string json)
        where T : class, ISystemSetting<T>
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, KmicJsonOptions.Stored) ?? T.Default;
        }
        catch (JsonException)
        {
            return T.Default;
        }
    }

    private T Read<T>(string key)
        where T : class, ISystemSetting<T> =>
        Values.TryGetValue(key, out var json) ? ParseOrDefault<T>(json) : T.Default;

    [JsonIgnore]
    internal int TypedCount => _typed.Count;
}

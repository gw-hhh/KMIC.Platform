using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Application.Settings;

namespace Kmic.Configuration.Infrastructure.Settings;

// 不可变的系统设置缓存快照，反序列化失败时回退默认值
[ImmutableObject(true)]
internal sealed class SettingsSnapshot(IReadOnlyDictionary<string, string> values)
{
    private readonly ConcurrentDictionary<Type, object> _typed = new();

    public IReadOnlyDictionary<string, string> Values { get; } = values;

    public T Get<T>(string key)
        where T : class, ISystemSetting<T> =>
        (T)_typed.GetOrAdd(typeof(T), _ => Read<T>(key));

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

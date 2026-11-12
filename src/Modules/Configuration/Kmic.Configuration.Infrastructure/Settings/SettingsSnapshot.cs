using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Application.Settings;

namespace Kmic.Configuration.Infrastructure.Settings;

/// <summary>
/// 全部被修改过的系统设置值的快照，进缓存
/// </summary>
/// <remarks>
/// 不可变：值只在构造时给定。各类型读出来的结果在快照里按类型记着，同一个快照里每个类型只反序列化一次，
/// 业务代码拿到的是同一个不可变记录。这个记录表只是缓存，不进 JSON
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
    /// 取一个设置的值：被修改过就读出修改后的，否则或者存着的值已经读不出来时（设置的结构后来改过）用默认值
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

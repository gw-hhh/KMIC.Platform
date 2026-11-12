using Serilog.Core;
using Serilog.Events;

namespace Kmic.ServiceDefaults;

/// <summary>
/// 按属性名兜底脱敏
/// </summary>
/// <remarks>
/// 脱敏的主要依据是数据分类的声明；这里是最后一道网：名字里带 Password、Token、Secret 等字样的属性，
/// 不论值是什么都输出成 ***。对象和集合里嵌套的属性同样处理
/// </remarks>
internal sealed class SensitivePropertyMasker : ILogEventEnricher
{
    private const string Mask = "***";

    private static readonly string[] _sensitiveNames =
    [
        "password", "passwd", "pwd", "token", "secret", "authorization", "apikey", "credential", "cookie", "connectionstring",
    ];

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(propertyFactory);

        foreach (var (name, value) in logEvent.Properties.ToList())
        {
            var masked = IsSensitive(name) ? new ScalarValue(Mask) : Visit(value);
            if (!ReferenceEquals(masked, value))
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty(name, masked));
            }
        }
    }

    public static bool IsSensitive(string name) =>
        _sensitiveNames.Any(sensitive => name.Contains(sensitive, StringComparison.OrdinalIgnoreCase));

    // 没有需要遮盖的内容就原样返回同一个对象，调用方据此判断要不要更新属性
    private static LogEventPropertyValue Visit(LogEventPropertyValue value)
    {
        switch (value)
        {
            case StructureValue structure:
            {
                var properties = structure.Properties
                    .Select(p => IsSensitive(p.Name) ? new LogEventProperty(p.Name, new ScalarValue(Mask)) : new LogEventProperty(p.Name, Visit(p.Value)))
                    .ToList();
                return properties.Zip(structure.Properties).All(pair => ReferenceEquals(pair.First.Value, pair.Second.Value))
                    ? value
                    : new StructureValue(properties, structure.TypeTag);
            }

            case DictionaryValue dictionary:
            {
                var elements = dictionary.Elements
                    .Select(e => KeyValuePair.Create(e.Key, e.Key.Value is string key && IsSensitive(key) ? new ScalarValue(Mask) : Visit(e.Value)))
                    .ToList();
                return elements.Zip(dictionary.Elements).All(pair => ReferenceEquals(pair.First.Value, pair.Second.Value))
                    ? value
                    : new DictionaryValue(elements);
            }

            case SequenceValue sequence:
            {
                var elements = sequence.Elements.Select(Visit).ToList();
                return elements.Zip(sequence.Elements).All(pair => ReferenceEquals(pair.First, pair.Second))
                    ? value
                    : new SequenceValue(elements);
            }

            default:
                return value;
        }
    }
}

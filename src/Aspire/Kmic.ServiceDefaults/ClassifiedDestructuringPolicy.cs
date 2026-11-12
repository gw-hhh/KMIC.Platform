using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Observability;
using Microsoft.Extensions.Compliance.Classification;
using Serilog.Core;
using Serilog.Events;

namespace Kmic.ServiceDefaults;

/// <summary>
/// 按数据分类脱敏：标了 <see cref="SecretDataAttribute" />、<see cref="PersonalDataAttribute" /> 的属性在日志里不输出原值
/// </summary>
/// <remarks>
/// 日志规范禁止用 {@对象} 把整个对象写进日志，这里是最后一道网：真的写了，标注过的属性也不会泄露。
/// 没有任何标注的类型交给 Serilog 默认的解构，不增加开销
/// </remarks>
internal sealed class ClassifiedDestructuringPolicy : IDestructuringPolicy
{
    private static readonly ConcurrentDictionary<Type, ClassifiedType?> _types = new();

    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(propertyValueFactory);

        var type = value.GetType();
        if (_types.GetOrAdd(type, Read) is not { } classified)
        {
            result = null;
            return false;
        }

        var properties = new List<LogEventProperty>(classified.Properties.Count);
        foreach (var (property, classification) in classified.Properties)
        {
            var raw = property.GetValue(value);
            LogEventPropertyValue member = classification is { } kind && raw is not null
                ? new ScalarValue(KmicRedactors.Redact(Convert.ToString(raw, System.Globalization.CultureInfo.InvariantCulture) ?? "", kind))
                : propertyValueFactory.CreatePropertyValue(raw, destructureObjects: true);
            properties.Add(new LogEventProperty(property.Name, member));
        }

        result = new StructureValue(properties, type.Name);
        return true;
    }

    // 只处理有标注的普通对象；字符串、集合、枚举这些交给默认解构
    private static ClassifiedType? Read(Type type)
    {
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
        {
            return null;
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .Select(p => (Property: p, Classification: p.GetCustomAttribute<DataClassificationAttribute>(inherit: true)?.Classification))
            .ToList();
        return properties.Any(p => p.Classification is not null) ? new ClassifiedType(properties) : null;
    }

    private sealed record ClassifiedType(IReadOnlyList<(PropertyInfo Property, DataClassification? Classification)> Properties);
}

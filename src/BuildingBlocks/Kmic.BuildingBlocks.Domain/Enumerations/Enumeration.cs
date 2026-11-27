using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Kmic.BuildingBlocks.Domain.Enumerations;

/// <summary>智能枚举基类；派生类须声明为 sealed 且构造函数私有，枚举值须为 public static readonly 字段。</summary>
[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "采用自引用泛型以便直接通过 SampleStatus.FromCode(...) 调用，无需显式指定类型参数")]
public abstract class Enumeration<TSelf> : IEnumeration, IEquatable<TSelf>
    where TSelf : Enumeration<TSelf>
{
    /// <summary>初始化枚举取值，由派生类私有构造函数调用。</summary>
    protected Enumeration(string code, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Code = code;
        Name = name;
    }

    /// <summary>按声明顺序返回全部枚举取值。</summary>
    public static IReadOnlyList<TSelf> All => Registry.All;

    public string Code { get; }

    public string Name { get; }

    public static bool operator ==(Enumeration<TSelf>? left, Enumeration<TSelf>? right) => Equals(left, right);

    public static bool operator !=(Enumeration<TSelf>? left, Enumeration<TSelf>? right) => !Equals(left, right);

    /// <summary>按编码获取枚举值，不存在时抛出异常。</summary>
    public static TSelf FromCode(string code) =>
        TryFromCode(code, out var value)
            ? value
            : throw new InvalidOperationException($"{typeof(TSelf).Name} 没有编码为“{code}”的取值。");

    /// <summary>尝试按编码获取枚举值。</summary>
    public static bool TryFromCode(string? code, [NotNullWhen(true)] out TSelf? value)
    {
        if (code is null)
        {
            value = null;
            return false;
        }

        return Registry.ByCode.TryGetValue(code, out value);
    }

    public bool Equals(TSelf? other) => other is not null && string.Equals(Code, other.Code, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is TSelf other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Code);

    public override string ToString() => Code;

    // 延迟至嵌套类首次访问时收集，确保派生类的静态字段均已完成初始化
    private static class Registry
    {
        public static readonly IReadOnlyList<TSelf> All = Collect();

        public static readonly FrozenDictionary<string, TSelf> ByCode = All.ToFrozenDictionary(x => x.Code, StringComparer.Ordinal);

        private static TSelf[] Collect()
        {
            var values = typeof(TSelf)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(field => field.FieldType == typeof(TSelf))
                .OrderBy(field => field.MetadataToken)
                .Select(field => (TSelf?)field.GetValue(null)
                    ?? throw new InvalidOperationException($"{typeof(TSelf).Name}.{field.Name} 尚未初始化。"))
                .ToArray();

            var duplicated = values.GroupBy(x => x.Code, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1);
            if (duplicated is not null)
            {
                throw new InvalidOperationException($"{typeof(TSelf).Name} 的编码“{duplicated.Key}”重复。");
            }

            return values;
        }
    }
}

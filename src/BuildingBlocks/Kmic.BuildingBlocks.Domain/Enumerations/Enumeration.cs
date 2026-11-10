using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Kmic.BuildingBlocks.Domain.Enumerations;

/// <summary>
/// 智能枚举基类：以英文编码存储和传输，可以带行为
/// </summary>
/// <remarks>
/// 派生类必须 sealed、构造函数私有、实例为 public static readonly 字段；All 按字段声明顺序排列
/// </remarks>
[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "自引用泛型：调用处写 SampleStatus.FromCode(...)，不需要写出类型参数")]
public abstract class Enumeration<TSelf> : IEnumeration, IEquatable<TSelf>
    where TSelf : Enumeration<TSelf>
{
    /// <summary>
    /// 由派生类的私有构造函数调用
    /// </summary>
    protected Enumeration(string code, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Code = code;
        Name = name;
    }

    /// <summary>
    /// 全部取值，按字段声明顺序
    /// </summary>
    public static IReadOnlyList<TSelf> All => Registry.All;

    /// <inheritdoc />
    public string Code { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// 相等（按编码）
    /// </summary>
    public static bool operator ==(Enumeration<TSelf>? left, Enumeration<TSelf>? right) => Equals(left, right);

    /// <summary>
    /// 不相等（按编码）
    /// </summary>
    public static bool operator !=(Enumeration<TSelf>? left, Enumeration<TSelf>? right) => !Equals(left, right);

    /// <summary>
    /// 按编码取值；找不到说明数据已损坏，属于程序缺陷，会抛出异常
    /// </summary>
    public static TSelf FromCode(string code) =>
        TryFromCode(code, out var value)
            ? value
            : throw new InvalidOperationException($"{typeof(TSelf).Name} 没有编码为“{code}”的取值。");

    /// <summary>
    /// 按编码取值；用于外部输入，找不到时返回 false
    /// </summary>
    public static bool TryFromCode(string? code, [NotNullWhen(true)] out TSelf? value)
    {
        if (code is null)
        {
            value = null;
            return false;
        }

        return Registry.ByCode.TryGetValue(code, out value);
    }

    /// <inheritdoc />
    public bool Equals(TSelf? other) => other is not null && string.Equals(Code, other.Code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is TSelf other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Code);

    /// <inheritdoc />
    public override string ToString() => Code;

    // 放在嵌套静态类中：等派生类的静态字段全部初始化之后，第一次访问 All 时才反射收集
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

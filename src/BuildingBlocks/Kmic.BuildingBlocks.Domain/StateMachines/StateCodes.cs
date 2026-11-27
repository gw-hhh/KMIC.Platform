using System.Collections;
using System.Reflection;
using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.BuildingBlocks.Domain.StateMachines;

// 提取状态与动作的编码及全集，兼容智能枚举与普通枚举
internal static class StateCodes
{
    public static void EnsureSupported<T>()
    {
        if (!typeof(T).IsEnum && !typeof(IEnumeration).IsAssignableFrom(typeof(T)))
        {
            throw new NotSupportedException($"{typeof(T).Name} 不是智能枚举或普通枚举。");
        }
    }

    public static string Of<T>(T value)
        where T : notnull =>
        value switch
        {
            IEnumeration enumeration => enumeration.Code,
            Enum member => member.ToString(),
            _ => throw new ArgumentException($"{typeof(T).Name} 不是智能枚举或普通枚举。", nameof(value)),
        };

    public static IReadOnlyList<T> AllValues<T>()
    {
        if (typeof(T).IsEnum)
        {
            return [.. Enum.GetValues(typeof(T)).Cast<T>()];
        }

        var all = typeof(T).GetProperty("All", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            ?.GetValue(null) as IEnumerable
            ?? throw new InvalidOperationException($"{typeof(T).Name} 没有 All 属性。");
        return [.. all.Cast<T>()];
    }
}

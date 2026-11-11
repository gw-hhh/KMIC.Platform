using System.Reflection;
using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.BuildingBlocks.Presentation.OpenApi;

/// <summary>
/// 读取智能枚举的全部取值
/// </summary>
internal static class EnumerationSchemas
{
    public static bool IsEnumeration(Type type) => Base(type) is not null;

    /// <summary>
    /// 全部取值，按声明顺序
    /// </summary>
    public static IReadOnlyList<IEnumeration> Members(Type type)
    {
        var all = Base(type)?.GetProperty("All", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            ?? throw new InvalidOperationException($"{type.Name} 不是智能枚举。");
        return [.. ((System.Collections.IEnumerable)all.GetValue(null)!).Cast<IEnumeration>()];
    }

    // 智能枚举派生自 Enumeration<TSelf>，取值表在这个泛型基类上
    private static Type? Base(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Enumeration<>))
            {
                return current;
            }
        }

        return null;
    }
}

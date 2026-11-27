using System.Reflection;
using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.BuildingBlocks.Presentation.OpenApi;

internal static class EnumerationSchemas
{
    public static bool IsEnumeration(Type type) => Base(type) is not null;

    // 按声明顺序返回智能枚举的全部取值
    public static IReadOnlyList<IEnumeration> Members(Type type)
    {
        var all = Base(type)?.GetProperty("All", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            ?? throw new InvalidOperationException($"{type.Name} 不是智能枚举。");
        return [.. ((System.Collections.IEnumerable)all.GetValue(null)!).Cast<IEnumeration>()];
    }

    // 智能枚举均派生自 Enumeration<TSelf>，静态取值集合定义于该泛型基类中
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

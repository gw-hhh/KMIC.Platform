using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 编译时生成的实体属性注释
/// </summary>
internal static class GeneratedComments
{
    private const string TypeName = "Kmic.Generated.EntityComments";

    private static readonly ConcurrentDictionary<Assembly, IReadOnlyDictionary<string, string>> _byAssembly = new();

    public static string? Of(PropertyInfo? property)
    {
        if (property?.DeclaringType is not { } type)
        {
            return null;
        }

        return _byAssembly.GetOrAdd(type.Assembly, Load).TryGetValue($"{type.FullName}.{property.Name}", out var comment) ? comment : null;
    }

    private static IReadOnlyDictionary<string, string> Load(Assembly assembly) =>
        assembly.GetType(TypeName)?.GetProperty("Properties", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as IReadOnlyDictionary<string, string>
        ?? FrozenDictionary<string, string>.Empty;
}

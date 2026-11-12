using System.Reflection;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.ArchitectureTests.Rules;

/// <summary>
/// 消息与响应的内容：只出现原始类型、强类型 ID、智能枚举、其他 DTO，不出现领域实体和值对象
/// </summary>
internal static class MessageContentRules
{
    /// <summary>
    /// 命令、查询和它们的响应里不出现模块 Domain 里的实体、值对象
    /// </summary>
    /// <param name="types">要检查的类型，通常是模块 Application 程序集里的全部类型</param>
    /// <param name="domainAssembly">模块的 Domain 程序集，里面的类型除了强类型 ID 和智能枚举都算领域类型</param>
    public static IReadOnlyList<string> NoDomainTypes(IEnumerable<Type> types, Assembly domainAssembly)
    {
        var problems = new List<string>();
        foreach (var message in types.Where(t => t is { IsClass: true, IsAbstract: false } && IsMessage(t)))
        {
            var checkedTypes = new List<Type> { message };
            if (ResponseOf(message) is { } response)
            {
                checkedTypes.Add(response);
            }

            foreach (var type in checkedTypes)
            {
                problems.AddRange(PropertyTypes(type)
                    .Where(p => p.Assembly == domainAssembly && !typeof(IStronglyTypedId).IsAssignableFrom(p) && !typeof(IEnumeration).IsAssignableFrom(p))
                    .Select(p => $"{type.FullName}：消息和响应里不能出现领域类型 {p.Name}，用原始类型、强类型 ID、智能枚举或 DTO。"));
            }
        }

        return problems;
    }

    private static bool IsMessage(Type type) =>
        typeof(ICommand).IsAssignableFrom(type)
        || type.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(ICommand<>) || i.GetGenericTypeDefinition() == typeof(IQuery<>)));

    private static Type? ResponseOf(Type message) =>
        message.GetInterfaces()
            .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(ICommand<>) || i.GetGenericTypeDefinition() == typeof(IQuery<>)))
            .Select(i => i.GetGenericArguments()[0])
            .FirstOrDefault();

    // 属性类型展开泛型参数与数组元素，再递归本程序集里的 DTO
    private static IEnumerable<Type> PropertyTypes(Type type) => PropertyTypes(type, []);

    private static IEnumerable<Type> PropertyTypes(Type type, HashSet<Type> seen)
    {
        if (!seen.Add(type))
        {
            yield break;
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            foreach (var inner in Unwrap(property.PropertyType))
            {
                yield return inner;

                if (inner.Namespace?.StartsWith("Kmic.", StringComparison.Ordinal) == true && inner.IsClass)
                {
                    foreach (var nested in PropertyTypes(inner, seen))
                    {
                        yield return nested;
                    }
                }
            }
        }
    }

    private static IEnumerable<Type> Unwrap(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (underlying.IsArray)
        {
            foreach (var element in Unwrap(underlying.GetElementType()!))
            {
                yield return element;
            }

            yield break;
        }

        yield return underlying;
        if (underlying.IsGenericType)
        {
            foreach (var argument in underlying.GetGenericArguments().SelectMany(Unwrap))
            {
                yield return argument;
            }
        }
    }
}

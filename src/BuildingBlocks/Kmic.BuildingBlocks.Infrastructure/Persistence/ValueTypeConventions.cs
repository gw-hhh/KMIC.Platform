using System.Reflection;
using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.BuildingBlocks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

// 须在模型构建前注册
internal static class ValueTypeConventions
{
    // 智能枚举编码列统一映射为 varchar(64)
    public const int CodeLength = 64;

    public static void Register(ModelConfigurationBuilder configurationBuilder, IEnumerable<Assembly> assemblies)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(CodeLength);

        foreach (var type in assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => t is { IsClass: true, IsAbstract: false }))
        {
            if (IsEnumeration(type))
            {
                configurationBuilder.Properties(type)
                    .HaveConversion(typeof(EnumerationConverter<>).MakeGenericType(type))
                    .HaveMaxLength(CodeLength);
            }
            else if (SingleValueType(type) is { } valueType)
            {
                configurationBuilder.Properties(type).HaveConversion(typeof(SingleValueConverter<,>).MakeGenericType(type, valueType));
            }
        }
    }

    public static bool IsEnumeration(Type type) =>
        type.BaseType is { IsGenericType: true } baseType && baseType.GetGenericTypeDefinition() == typeof(Enumeration<>);

    // 收集枚举的全部有效编码值，用于生成数据库检查约束
    public static IReadOnlyList<string> CodesOf(Type type)
    {
        if (type.IsEnum)
        {
            return Enum.GetNames(type);
        }

        var all = (IEnumerable<IEnumeration>)typeof(Enumeration<>).MakeGenericType(type).GetProperty(nameof(Enumeration<>.All))!.GetValue(null)!;
        return [.. all.Select(e => e.Code)];
    }

    private static Type? SingleValueType(Type type) =>
        type.GetInterfaces()
            .SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISingleValueObject<>))
            ?.GetGenericArguments()[0];
}

using System.Reflection;
using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.BuildingBlocks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 构建模型之前登记的类型映射：智能枚举与普通枚举存为编码，单值值对象存为原始值
/// </summary>
/// <remarks>
/// 必须在构建模型之前登记，否则 EF 会把智能枚举、单值值对象这类引用类型当成实体去发现
/// </remarks>
internal static class ValueTypeConventions
{
    // 05-01 D5：编码列统一 varchar(64)
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

    // 编码列的全部取值，供 CHECK 约束使用
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

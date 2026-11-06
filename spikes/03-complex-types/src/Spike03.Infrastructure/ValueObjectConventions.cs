using Microsoft.EntityFrameworkCore;
using Spike03.Domain;

namespace Spike03.Infrastructure;

/// <summary>
/// 框架对值对象的映射约定（05-03 第 3.4 节）
/// </summary>
public static class ValueObjectConventions
{
    public static void AddValueObjectConventions(this ModelConfigurationBuilder configurationBuilder, params IEnumerable<System.Reflection.Assembly> domainAssemblies)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        var singleValueTypes = domainAssemblies
            .SelectMany(a => a.GetTypes())
            .Select(t => (Type: t, Value: SingleValueType(t)))
            .Where(p => p.Value is not null);
        foreach (var (type, value) in singleValueTypes)
        {
            configurationBuilder.Properties(type).HaveConversion(typeof(SingleValueConverter<,>).MakeGenericType(type, value!));
        }

        configurationBuilder.Conventions.Add(_ => new ComplexColumnNamingConvention());
    }

    private static Type? SingleValueType(Type type) =>
        type.GetInterfaces()
            .SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISingleValueObject<>))
            ?.GetGenericArguments()[0];
}

using System.Reflection;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Settings;

namespace Kmic.Configuration.Application.Settings;

/// <summary>
/// 从设置的值类型读出成员清单
/// </summary>
internal static class SettingFields
{
    public static IReadOnlyList<SettingFieldInfo> Of(Type valueType)
    {
        ArgumentNullException.ThrowIfNull(valueType);

        return
        [
            .. valueType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0)
                .Select(p =>
                {
                    var declared = p.GetCustomAttribute<SettingFieldAttribute>();
                    var type = p.PropertyType;
                    var isArray = type.IsArray;
                    var element = isArray ? type.GetElementType()! : type;
                    var underlying = Nullable.GetUnderlyingType(element);
                    var isNullable = underlying is not null || (!element.IsValueType && new NullabilityInfoContext().Create(p).ReadState == NullabilityState.Nullable);
                    return new SettingFieldInfo(
                        JsonNamingPolicy.CamelCase.ConvertName(p.Name),
                        TypeName(underlying ?? element),
                        declared?.DisplayName ?? p.Name,
                        declared?.Unit,
                        isNullable,
                        isArray);
                }),
        ];
    }

    private static string TypeName(Type type) => type switch
    {
        _ when type == typeof(bool) => "bool",
        _ when type == typeof(int) => "int",
        _ when type == typeof(long) => "long",
        _ when type == typeof(decimal) => "decimal",
        _ when type == typeof(double) => "double",
        _ when type == typeof(string) => "string",
        _ when type == typeof(Guid) => "guid",
        _ when type == typeof(DateOnly) => "date",
        _ when type == typeof(TimeOnly) => "time",
        _ when type == typeof(DateTimeOffset) => "dateTime",
        _ when type == typeof(TimeSpan) => "timeSpan",
        _ => type.Name,
    };
}

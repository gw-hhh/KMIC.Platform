using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 设置声明的校验规则
/// </summary>
internal static partial class DeclaredValueRules
{
    // 限制成员类型，序列化结果才稳定，管理界面才能直接编辑
    private static readonly FrozenSet<Type> _scalars = new[]
    {
        typeof(bool), typeof(int), typeof(long), typeof(decimal), typeof(double), typeof(string), typeof(Guid),
        typeof(DateOnly), typeof(TimeOnly), typeof(DateTimeOffset), typeof(TimeSpan),
    }.ToFrozenSet();

    public static void CheckKey(Type type, string key, List<string> problems)
    {
        var module = ModuleNames.Of(type);
        if (!KeyPattern().IsMatch(key))
        {
            problems.Add($"{type.Name}：键 {key} 必须是以“.”分隔的大驼峰段，至少两段。");
        }
        else if (!key.StartsWith(module + ".", StringComparison.Ordinal))
        {
            problems.Add($"{type.Name}：键 {key} 的第一段必须是本模块名 {module}。");
        }
    }

    public static void CheckShape(Type type, List<string> problems)
    {
        if (!type.IsSealed || type.GetMethod("<Clone>$") is null)
        {
            problems.Add($"{type.Name}：值类型必须是 sealed record。");
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.SetMethod is { } setter && !setter.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)))
            {
                problems.Add($"{type.Name}.{property.Name}：值必须不可变，属性只能 init。");
            }

            if (!IsAllowed(property.PropertyType))
            {
                problems.Add($"{type.Name}.{property.Name}：类型 {property.PropertyType.Name} 不能用于设置值，只能用基础类型、decimal、string、Guid、日期时间类型及它们的数组。");
            }
        }
    }

    public static void CheckDefault(Type type, object defaultValue, Func<object, Result> validate, List<string> problems)
    {
        if (validate(defaultValue) is { IsFailure: true } failed)
        {
            problems.Add($"{type.Name}：默认值没有通过自身的 Validate（{failed.Error.Code}）。");
        }
    }

    // 不用 MakeGenericType 构造接口再比较：类型不满足约束时它会直接抛异常
    public static bool ImplementsSelf(Type type, Type interfaceDefinition) =>
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceDefinition && i.GenericTypeArguments[0] == type);

    private static bool IsAllowed(Type type) =>
        _scalars.Contains(Nullable.GetUnderlyingType(type) ?? type)
        || (type.IsArray && _scalars.Contains(type.GetElementType()!));

    [GeneratedRegex(@"^[A-Z][A-Za-z0-9]*(\.[A-Z][A-Za-z0-9]*)+$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex KeyPattern();
}

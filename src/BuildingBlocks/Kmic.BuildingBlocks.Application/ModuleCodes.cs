using System.Reflection;
using System.Text.RegularExpressions;

namespace Kmic.BuildingBlocks.Application;

/// <summary>
/// 读取“模块.名称”形式的编码常量：范围对象、字典类型都这样声明
/// </summary>
internal static partial class ModuleCodes
{
    /// <summary>
    /// 读出分组类型中标了 TAttribute 的 string 常量；格式不对、不属于本模块的记为问题并跳过
    /// </summary>
    public static IEnumerable<(string Code, TAttribute Attribute)> Read<TAttribute>(Type group, string module, string kind, List<string> problems)
        where TAttribute : Attribute
    {
        foreach (var field in group.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            var attribute = field.GetCustomAttribute<TAttribute>();
            if (!field.IsLiteral || field.FieldType != typeof(string) || attribute is null)
            {
                problems.Add($"{group.Name}.{field.Name}：{kind}必须是标了 [{typeof(TAttribute).Name[..^"Attribute".Length]}] 的 string 常量。");
                continue;
            }

            if (field.GetRawConstantValue() is not string code || !Pattern().IsMatch(code))
            {
                problems.Add($"{group.Name}.{field.Name}：{kind} {field.GetRawConstantValue()} 不符合“模块.名称”格式（两段，每段大驼峰）。");
            }
            else if (!code.StartsWith(module + ".", StringComparison.Ordinal))
            {
                problems.Add($"{group.Name}.{field.Name}：{kind} {code} 的第一段必须是本模块名 {module}。");
            }
            else
            {
                yield return (code, attribute);
            }
        }
    }

    /// <summary>
    /// 编码全系统唯一，显示名在模块内唯一
    /// </summary>
    public static void CheckUnique<T>(IReadOnlyList<T> items, Func<T, string> code, Func<T, (string Module, string DisplayName)> name, string kind, List<string> problems)
    {
        foreach (var duplicated in items.GroupBy(code, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"{kind} {duplicated.Key} 重复声明。");
        }

        foreach (var duplicated in items.GroupBy(name).Where(g => g.Count() > 1))
        {
            problems.Add($"模块 {duplicated.Key.Module} 中{kind}显示名“{duplicated.Key.DisplayName}”重复。");
        }
    }

    [GeneratedRegex(@"^[A-Z][A-Za-z0-9]*\.[A-Z][A-Za-z0-9]*$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex Pattern();
}

using System.Collections.Frozen;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Kmic.BuildingBlocks.Application.Numbering;

/// <summary>
/// 编号规则目录
/// </summary>
public sealed partial class NumberRuleCatalog
{
    // 和计数表 Rule 列的长度一致
    private const int MaxNameLength = 100;

    private readonly FrozenDictionary<string, NumberRule> _byName;

    private NumberRuleCatalog(IEnumerable<NumberRule> rules) => _byName = rules.ToFrozenDictionary(r => r.Name, StringComparer.Ordinal);

    /// <summary>
    /// 全部规则
    /// </summary>
    public IReadOnlyCollection<NumberRule> All => _byName.Values;

    /// <summary>
    /// 从程序集中读取
    /// </summary>
    public static NumberRuleCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()));
    }

    /// <summary>
    /// 从给定类型中读取
    /// </summary>
    public static NumberRuleCatalog Build(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        var problems = new List<string>();
        var rules = new List<NumberRule>();
        foreach (var type in types.Distinct())
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (field.GetCustomAttribute<NumberRuleAttribute>() is { } declaration && Read(type, field, declaration, problems) is { } rule)
                {
                    rules.Add(rule);
                }
            }
        }

        foreach (var duplicated in rules.GroupBy(r => r.Name, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"编号规则 {duplicated.Key} 重复声明。");
        }

        return problems.Count > 0
            ? throw new DeclarationException("编号规则声明", problems)
            : new NumberRuleCatalog(rules);
    }

    /// <summary>
    /// 按名称取规则，不存在时抛异常
    /// </summary>
    public NumberRule Get(string name) =>
        _byName.TryGetValue(name, out var rule) ? rule : throw new InvalidOperationException($"编号规则 {name} 不存在。");

    private static NumberRule? Read(Type type, FieldInfo field, NumberRuleAttribute declaration, List<string> problems)
    {
        var location = $"{type.Name}.{field.Name}";
        if (!field.IsPublic || !field.IsLiteral || field.FieldType != typeof(string))
        {
            problems.Add($"{location}：[NumberRule] 只能标在 public string 常量上。");
            return null;
        }

        var name = (string)field.GetRawConstantValue()!;
        var module = ModuleNames.Of(type);
        if (!NamePattern().IsMatch(name) || name.Length > MaxNameLength)
        {
            problems.Add($"{location}：规则名 {name} 必须是“模块.名称”形式（每段大驼峰），不超过 {MaxNameLength} 个字符。");
            return null;
        }

        if (!name.StartsWith(module + ".", StringComparison.Ordinal))
        {
            problems.Add($"{location}：规则名 {name} 的第一段必须是本模块名 {module}。");
            return null;
        }

        return NumberRule.Parse(name, module, declaration, problems);
    }

    [GeneratedRegex(@"^[A-Z][A-Za-z0-9]*(\.[A-Z][A-Za-z0-9]*)+$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex NamePattern();
}

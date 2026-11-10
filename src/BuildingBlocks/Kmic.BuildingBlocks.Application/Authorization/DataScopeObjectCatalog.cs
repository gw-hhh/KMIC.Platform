using System.Collections.Frozen;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 全部模块的范围对象目录：角色上的范围覆盖、实体的 HasDataScope 都只能引用其中的编码
/// </summary>
public sealed partial class DataScopeObjectCatalog
{
    private readonly FrozenDictionary<string, DataScopeObjectDefinition> _byCode;

    private DataScopeObjectCatalog(IEnumerable<DataScopeObjectDefinition> scopeObjects) =>
        _byCode = scopeObjects.ToFrozenDictionary(s => s.Code, StringComparer.Ordinal);

    /// <summary>
    /// 全部范围对象
    /// </summary>
    public IReadOnlyCollection<DataScopeObjectDefinition> All => _byCode.Values;

    /// <summary>
    /// 扫描程序集中标了 [DataScopeObjectGroup] 的常量类
    /// </summary>
    public static DataScopeObjectCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => t.IsDefined(typeof(DataScopeObjectGroupAttribute), inherit: false)));
    }

    /// <summary>
    /// 读取并校验格式、归属与唯一性
    /// </summary>
    public static DataScopeObjectCatalog Build(IEnumerable<Type> groupTypes)
    {
        ArgumentNullException.ThrowIfNull(groupTypes);

        var problems = new List<string>();
        var scopeObjects = groupTypes.Distinct().SelectMany(type => ReadGroup(type, problems)).ToList();

        foreach (var duplicated in scopeObjects.GroupBy(s => s.Code, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"范围对象 {duplicated.Key} 重复声明。");
        }

        foreach (var duplicated in scopeObjects.GroupBy(s => (s.Module, s.DisplayName)).Where(g => g.Count() > 1))
        {
            problems.Add($"模块 {duplicated.Key.Module} 中范围对象显示名“{duplicated.Key.DisplayName}”重复。");
        }

        return problems.Count > 0
            ? throw new DeclarationException("范围对象声明", problems)
            : new DataScopeObjectCatalog(scopeObjects);
    }

    /// <summary>
    /// 范围对象是否存在
    /// </summary>
    public bool Contains(string code) => _byCode.ContainsKey(code);

    private static IEnumerable<DataScopeObjectDefinition> ReadGroup(Type type, List<string> problems)
    {
        var group = type.GetCustomAttribute<DataScopeObjectGroupAttribute>()!;
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            var attribute = field.GetCustomAttribute<DataScopeObjectAttribute>();
            if (!field.IsLiteral || field.FieldType != typeof(string) || attribute is null)
            {
                problems.Add($"{type.Name}.{field.Name}：范围对象必须是标了 [DataScopeObject] 的 string 常量。");
                continue;
            }

            var code = (string)field.GetRawConstantValue()!;
            if (!CodePattern().IsMatch(code))
            {
                problems.Add($"{type.Name}.{field.Name}：范围对象 {code} 不符合“模块.对象”格式（两段，每段大驼峰）。");
                continue;
            }

            if (!code.StartsWith(group.Module + ".", StringComparison.Ordinal))
            {
                problems.Add($"{type.Name}.{field.Name}：范围对象 {code} 的第一段必须是本模块名 {group.Module}。");
                continue;
            }

            yield return new DataScopeObjectDefinition(code, group.Module, attribute.DisplayName);
        }
    }

    [GeneratedRegex(@"^[A-Z][A-Za-z0-9]*\.[A-Z][A-Za-z0-9]*$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex CodePattern();
}

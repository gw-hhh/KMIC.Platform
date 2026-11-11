using System.Collections.Frozen;
using System.Reflection;

namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>
/// 字典类型目录
/// </summary>
public sealed class DictionaryTypeCatalog
{
    private readonly FrozenDictionary<string, DictionaryTypeDefinition> _byCode;

    private DictionaryTypeCatalog(IEnumerable<DictionaryTypeDefinition> types) =>
        _byCode = types.ToFrozenDictionary(t => t.Code, StringComparer.Ordinal);

    /// <summary>
    /// 全部字典类型
    /// </summary>
    public IReadOnlyCollection<DictionaryTypeDefinition> All => _byCode.Values;

    /// <summary>
    /// 从程序集中读取
    /// </summary>
    public static DictionaryTypeCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => t.IsDefined(typeof(DictionaryTypeGroupAttribute), inherit: false)));
    }

    /// <summary>
    /// 从分组类型中读取
    /// </summary>
    public static DictionaryTypeCatalog Build(IEnumerable<Type> groupTypes)
    {
        ArgumentNullException.ThrowIfNull(groupTypes);

        var problems = new List<string>();
        var types = groupTypes.Distinct()
            .SelectMany(group =>
            {
                var module = group.GetCustomAttribute<DictionaryTypeGroupAttribute>()?.Module
                    ?? throw new ArgumentException($"{group.Name} 没有标 [DictionaryTypeGroup]。", nameof(groupTypes));
                return ModuleCodes.Read<DictionaryTypeAttribute>(group, module, "字典类型", problems)
                    .Select(d => new DictionaryTypeDefinition(d.Code, module, d.Attribute.DisplayName, d.Attribute.IsHierarchical));
            })
            .ToList();
        ModuleCodes.CheckUnique(types, t => t.Code, t => (t.Module, t.DisplayName), "字典类型", problems);

        return problems.Count > 0 ? throw new DeclarationException("字典类型声明", problems) : new DictionaryTypeCatalog(types);
    }

    /// <summary>
    /// 是否包含该字典类型
    /// </summary>
    public bool Contains(string code) => _byCode.ContainsKey(code);
}

using System.Collections.Frozen;
using System.Reflection;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 数据范围对象目录
/// </summary>
public sealed class DataScopeObjectCatalog
{
    private readonly FrozenDictionary<string, DataScopeObjectDefinition> _byCode;

    private DataScopeObjectCatalog(IEnumerable<DataScopeObjectDefinition> scopeObjects) =>
        _byCode = scopeObjects.ToFrozenDictionary(s => s.Code, StringComparer.Ordinal);

    /// <summary>
    /// 全部范围对象
    /// </summary>
    public IReadOnlyCollection<DataScopeObjectDefinition> All => _byCode.Values;

    /// <summary>
    /// 从程序集中读取
    /// </summary>
    public static DataScopeObjectCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => t.IsDefined(typeof(DataScopeObjectGroupAttribute), inherit: false)));
    }

    /// <summary>
    /// 从分组类型中读取
    /// </summary>
    public static DataScopeObjectCatalog Build(IEnumerable<Type> groupTypes)
    {
        ArgumentNullException.ThrowIfNull(groupTypes);

        var problems = new List<string>();
        var scopeObjects = groupTypes.Distinct()
            .SelectMany(group =>
            {
                var module = group.GetCustomAttribute<DataScopeObjectGroupAttribute>()?.Module
                    ?? throw new ArgumentException($"{group.Name} 没有标 [DataScopeObjectGroup]。", nameof(groupTypes));
                return ModuleCodes.Read<DataScopeObjectAttribute>(group, module, "范围对象", problems)
                    .Select(d => new DataScopeObjectDefinition(d.Code, module, d.Attribute.DisplayName));
            })
            .ToList();
        ModuleCodes.CheckUnique(scopeObjects, s => s.Code, s => (s.Module, s.DisplayName), "范围对象", problems);

        return problems.Count > 0 ? throw new DeclarationException("范围对象声明", problems) : new DataScopeObjectCatalog(scopeObjects);
    }

    /// <summary>
    /// 是否包含该范围对象
    /// </summary>
    public bool Contains(string code) => _byCode.ContainsKey(code);
}

using System.Collections.Frozen;
using System.Reflection;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 全部模块的范围对象目录：角色上的范围覆盖、实体的 HasDataScope 都只能引用其中的编码
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
    /// 范围对象是否存在
    /// </summary>
    public bool Contains(string code) => _byCode.ContainsKey(code);
}

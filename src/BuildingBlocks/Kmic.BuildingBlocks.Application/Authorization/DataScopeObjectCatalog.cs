using System.Collections.Frozen;
using System.Reflection;

namespace Kmic.BuildingBlocks.Application.Authorization;

public sealed class DataScopeObjectCatalog
{
    private readonly FrozenDictionary<string, DataScopeObjectDefinition> _byCode;

    private DataScopeObjectCatalog(IEnumerable<DataScopeObjectDefinition> scopeObjects) =>
        _byCode = scopeObjects.ToFrozenDictionary(s => s.Code, StringComparer.Ordinal);

    public IReadOnlyCollection<DataScopeObjectDefinition> All => _byCode.Values;

    public static DataScopeObjectCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => t.IsDefined(typeof(DataScopeObjectGroupAttribute), inherit: false)));
    }

    public static DataScopeObjectCatalog Build(IEnumerable<Type> groupTypes)
    {
        ArgumentNullException.ThrowIfNull(groupTypes);

        var problems = new List<string>();
        var scopeObjects = groupTypes.Distinct()
            .SelectMany(group =>
            {
                var module = group.GetCustomAttribute<DataScopeObjectGroupAttribute>()?.Module
                    ?? throw new ArgumentException($"{group.Name} 未标注 [DataScopeObjectGroup]。", nameof(groupTypes));
                return ModuleCodes.Read<DataScopeObjectAttribute>(group, module, "范围对象", problems)
                    .Select(d => new DataScopeObjectDefinition(d.Code, module, d.Attribute.DisplayName));
            })
            .ToList();
        ModuleCodes.CheckUnique(scopeObjects, s => s.Code, s => (s.Module, s.DisplayName), "范围对象", problems);

        return problems.Count > 0 ? throw new DeclarationException("范围对象声明", problems) : new DataScopeObjectCatalog(scopeObjects);
    }

    public bool Contains(string code) => _byCode.ContainsKey(code);
}

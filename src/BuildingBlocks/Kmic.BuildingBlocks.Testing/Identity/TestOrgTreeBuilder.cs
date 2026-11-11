using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>
/// 测试组织树的一级
/// </summary>
public sealed class TestOrgTreeBuilder
{
    private readonly OrgUnitId? _parent;

    internal TestOrgTreeBuilder(OrgUnitId? parent, Dictionary<string, OrgUnitId> byName, Dictionary<OrgUnitId, OrgUnitId?> parents)
    {
        _parent = parent;
        ByName = byName;
        Parents = parents;
    }

    internal Dictionary<string, OrgUnitId> ByName { get; }

    internal Dictionary<OrgUnitId, OrgUnitId?> Parents { get; }

    /// <summary>
    /// 添加组织
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="children">下级组织</param>
    public TestOrgTreeBuilder Unit(string name, Action<TestOrgTreeBuilder>? children = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var id = OrgUnitId.New();
        if (!ByName.TryAdd(name, id))
        {
            throw new ArgumentException($"测试组织树中“{name}”重复。", nameof(name));
        }

        Parents.Add(id, _parent);
        children?.Invoke(new TestOrgTreeBuilder(id, ByName, Parents));
        return this;
    }
}

using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>
/// 声明测试组织树中的一级组织单元
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
    /// 在当前一级下增加组织单元
    /// </summary>
    /// <param name="name">名称，测试中按它取 ID</param>
    /// <param name="children">下级组织单元</param>
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

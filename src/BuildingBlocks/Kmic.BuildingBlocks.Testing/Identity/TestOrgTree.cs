using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>
/// 测试组织树
/// </summary>
/// <remarks>
/// 按名称建树，按名称取 ID
/// </remarks>
/// <example>
/// <code>
/// var org = TestOrgTree.Create(t => t.Unit("总部", hq => hq.Unit("华东", east => east.Unit("上海").Unit("杭州"))));
/// var shanghai = org["上海"];
/// </code>
/// </example>
public sealed class TestOrgTree : IOrgHierarchy
{
    private readonly Dictionary<string, OrgUnitId> _byName;
    private readonly Dictionary<OrgUnitId, OrgUnitId?> _parents;

    private TestOrgTree(Dictionary<string, OrgUnitId> byName, Dictionary<OrgUnitId, OrgUnitId?> parents, long version)
    {
        _byName = byName;
        _parents = parents;
        Version = version;
    }

    /// <summary>
    /// 空树
    /// </summary>
    public static TestOrgTree Empty { get; } = new([], [], 1);

    /// <inheritdoc />
    public long Version { get; }

    /// <summary>
    /// 按名称取组织 ID
    /// </summary>
    public OrgUnitId this[string name] =>
        _byName.TryGetValue(name, out var id) ? id : throw new KeyNotFoundException($"测试组织树中没有“{name}”。");

    /// <summary>
    /// 建树，同一棵树里名称不能重复
    /// </summary>
    /// <param name="build">从第一级开始声明组织</param>
    /// <param name="version">树版本号</param>
    public static TestOrgTree Create(Action<TestOrgTreeBuilder> build, long version = 1)
    {
        ArgumentNullException.ThrowIfNull(build);

        var builder = new TestOrgTreeBuilder(parent: null, byName: [], parents: []);
        build(builder);
        return new TestOrgTree(builder.ByName, builder.Parents, version);
    }

    /// <inheritdoc />
    public bool Exists(OrgUnitId id) => _parents.ContainsKey(id);

    /// <inheritdoc />
    public IReadOnlySet<OrgUnitId> SelfAndDescendants(OrgUnitId id)
    {
        EnsureExists(id);
        return _parents.Keys.Where(unit => unit == id || Ancestors(unit).Contains(id)).ToHashSet();
    }

    /// <inheritdoc />
    public IReadOnlyList<OrgUnitId> Ancestors(OrgUnitId id)
    {
        EnsureExists(id);
        var ancestors = new List<OrgUnitId>();
        for (var parent = _parents[id]; parent is { } current; parent = _parents[current])
        {
            ancestors.Add(current);
        }

        return ancestors;
    }

    private void EnsureExists(OrgUnitId id)
    {
        if (!Exists(id))
        {
            throw new KeyNotFoundException($"测试组织树中没有组织单元 {id}。");
        }
    }
}

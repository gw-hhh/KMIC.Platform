using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// 组织树快照：按树版本缓存，内容不可变
/// </summary>
/// <param name="Version">组织树的并发版本号</param>
/// <param name="Nodes">全部单元（含已停用）：ID 与上级 ID，根单元的上级为空</param>
internal sealed record OrgTreeSnapshot(long Version, IReadOnlyList<OrgNode> Nodes) : IOrgHierarchy
{
    private readonly Lazy<Index> _index = new(() => Index.Build(Nodes));

    public static OrgTreeSnapshot Empty { get; } = new(0, []);

    public bool Exists(OrgUnitId id) => _index.Value.Parent.ContainsKey(id.Value);

    public IReadOnlySet<OrgUnitId> SelfAndDescendants(OrgUnitId id)
    {
        var result = new HashSet<OrgUnitId>();
        if (!Exists(id))
        {
            return result;
        }

        var pending = new Stack<Guid>([id.Value]);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!result.Add(OrgUnitId.From(current)))
            {
                continue;
            }

            if (_index.Value.Children.TryGetValue(current, out var children))
            {
                foreach (var child in children)
                {
                    pending.Push(child);
                }
            }
        }

        return result;
    }

    public IReadOnlyList<OrgUnitId> Ancestors(OrgUnitId id)
    {
        var ancestors = new List<OrgUnitId>();
        var parent = _index.Value.Parent.GetValueOrDefault(id.Value);
        while (parent is { } parentId && ancestors.Count <= Nodes.Count)
        {
            ancestors.Add(OrgUnitId.From(parentId));
            parent = _index.Value.Parent.GetValueOrDefault(parentId);
        }

        return ancestors;
    }

    private sealed class Index
    {
        public required Dictionary<Guid, Guid?> Parent { get; init; }

        public required Dictionary<Guid, List<Guid>> Children { get; init; }

        public static Index Build(IReadOnlyList<OrgNode> nodes)
        {
            var parent = nodes.ToDictionary(n => n.Id, n => n.ParentId);
            var children = new Dictionary<Guid, List<Guid>>();
            foreach (var node in nodes.Where(n => n.ParentId is not null))
            {
                if (!children.TryGetValue(node.ParentId!.Value, out var list))
                {
                    list = [];
                    children[node.ParentId.Value] = list;
                }

                list.Add(node.Id);
            }

            return new Index { Parent = parent, Children = children };
        }
    }
}

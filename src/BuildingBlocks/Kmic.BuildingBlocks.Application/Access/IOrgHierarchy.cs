using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

public interface IOrgHierarchy
{
    long Version { get; }

    /// <summary>判断组织是否存在（含已停用组织）。</summary>
    bool Exists(OrgUnitId id);

    /// <summary>返回自身及全部下级组织（含已停用下级）。</summary>
    IReadOnlySet<OrgUnitId> SelfAndDescendants(OrgUnitId id);

    /// <summary>返回全部上级组织，按由近及远排序。</summary>
    IReadOnlyList<OrgUnitId> Ancestors(OrgUnitId id);
}

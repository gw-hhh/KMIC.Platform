using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 组织树
/// </summary>
public interface IOrgHierarchy
{
    /// <summary>
    /// 树版本号
    /// </summary>
    long Version { get; }

    /// <summary>
    /// 组织是否存在，含已停用的
    /// </summary>
    bool Exists(OrgUnitId id);

    /// <summary>
    /// 自身及全部下级，含已停用的下级
    /// </summary>
    IReadOnlySet<OrgUnitId> SelfAndDescendants(OrgUnitId id);

    /// <summary>
    /// 全部上级，由近到远
    /// </summary>
    IReadOnlyList<OrgUnitId> Ancestors(OrgUnitId id);
}

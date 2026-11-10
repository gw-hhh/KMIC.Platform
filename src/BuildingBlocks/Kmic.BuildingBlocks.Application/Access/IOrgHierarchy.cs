namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 某个树版本的组织单元树快照，按版本缓存
/// </summary>
public interface IOrgHierarchy
{
    /// <summary>
    /// 树版本，即组织树聚合的并发版本
    /// </summary>
    long Version { get; }

    /// <summary>
    /// 组织单元是否存在（含已停用）
    /// </summary>
    bool Exists(OrgUnitId id);

    /// <summary>
    /// 自身及全部下级，含已停用的下级：停用单元的历史数据仍要能被看到
    /// </summary>
    IReadOnlySet<OrgUnitId> SelfAndDescendants(OrgUnitId id);

    /// <summary>
    /// 全部上级，由近及远
    /// </summary>
    IReadOnlyList<OrgUnitId> Ancestors(OrgUnitId id);
}

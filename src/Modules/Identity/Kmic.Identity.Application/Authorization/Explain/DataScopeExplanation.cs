using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Authorization.Explain;

/// <summary>
/// 一个范围对象的数据范围及其来源
/// </summary>
/// <remarks>
/// 除“全部”外，本人负责的数据始终可见；组织单元只决定另外还能看到哪些组织的数据
/// </remarks>
/// <param name="ScopeObject">范围对象编码</param>
/// <param name="DisplayName">范围对象的显示名</param>
/// <param name="Level">等级</param>
/// <param name="LevelOverridden">等级是否为角色对这个范围对象单独设置的；为否时用角色的默认等级</param>
/// <param name="Anchors">组织范围的起点；“本人”“全部”不按组织判断，为空集合</param>
/// <param name="AnchorsFromGrant">起点是否用了用户角色授予上设置的组织锚点；为否时用用户所属组织</param>
/// <param name="OrgUnitCount">能看到的组织单元数，包括展开的下级；“本人”“全部”为空</param>
public sealed record DataScopeExplanation(
    string ScopeObject,
    string DisplayName,
    RoleScopeLevel Level,
    bool LevelOverridden,
    IReadOnlyList<ExplainedOrgUnit> Anchors,
    bool AnchorsFromGrant,
    int? OrgUnitCount);

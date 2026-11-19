using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Authorization.Explain;

/// <summary>
/// 一个范围对象的数据范围及其来源
/// </summary>
/// <remarks>
/// 除“全部”外，本人负责的数据始终可见
/// </remarks>
/// <param name="ScopeObject">范围对象编码</param>
/// <param name="DisplayName">范围对象的显示名</param>
/// <param name="Level">等级</param>
/// <param name="LevelOverridden">等级是否为角色对这个范围对象单独设置的</param>
/// <param name="Anchors">组织范围的起点，为空集合</param>
/// <param name="AnchorsFromGrant">起点是否用了用户角色授予上设置的组织锚点</param>
/// <param name="OrgUnitCount">能看到的组织单元数，“本人”“全部”为空</param>
public sealed record DataScopeExplanation(
    string ScopeObject,
    string DisplayName,
    RoleScopeLevel Level,
    bool LevelOverridden,
    IReadOnlyList<ExplainedOrgUnit> Anchors,
    bool AnchorsFromGrant,
    int? OrgUnitCount);

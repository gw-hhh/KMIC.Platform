using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Authorization.Explain;

/// <summary>范围对象的数据范围及其来源（除“全部”外，本人负责的数据始终可见）</summary>
/// <param name="ScopeObject">范围对象编码</param>
/// <param name="DisplayName">范围对象显示名</param>
/// <param name="Level">等级</param>
/// <param name="LevelOverridden">是否覆盖等级（由角色针对该范围对象单独设置）</param>
/// <param name="Anchors">组织范围起点（无起点时为空集合）</param>
/// <param name="AnchorsFromGrant">是否采用授予锚点（由用户角色授予设置）</param>
/// <param name="OrgUnitCount">可见组织单元数（“本人”与“全部”时为空）</param>
public sealed record DataScopeExplanation(
    string ScopeObject,
    string DisplayName,
    RoleScopeLevel Level,
    bool LevelOverridden,
    IReadOnlyList<ExplainedOrgUnit> Anchors,
    bool AnchorsFromGrant,
    int? OrgUnitCount);

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 数据权限等级
/// </summary>
public enum ScopeLevel
{
    /// <summary>
    /// 本人
    /// </summary>
    Self,

    /// <summary>
    /// 本人和本组织
    /// </summary>
    OrgUnit,

    /// <summary>
    /// 本人、本组织及下级
    /// </summary>
    OrgUnitAndBelow,

    /// <summary>
    /// 全部
    /// </summary>
    All,
}

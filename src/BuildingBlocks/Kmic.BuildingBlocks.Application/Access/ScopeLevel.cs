namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 数据范围等级，等级之间是累加关系
/// </summary>
public enum ScopeLevel
{
    /// <summary>
    /// 本人
    /// </summary>
    Self,

    /// <summary>
    /// 本人 + 锚点组织单元
    /// </summary>
    OrgUnit,

    /// <summary>
    /// 本人 + 锚点组织单元及其全部下级
    /// </summary>
    OrgUnitAndBelow,

    /// <summary>
    /// 全部
    /// </summary>
    All,
}

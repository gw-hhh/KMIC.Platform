namespace Kmic.Identity.Domain.Roles;

/// <summary>
/// 角色上配置的数据范围等级
/// </summary>
/// <remarks>
/// 与框架里访问上下文用的 ScopeLevel 一一对应；领域层不引用应用层，所以单独定义，由基础设施层转换
/// </remarks>
public enum RoleScopeLevel
{
    /// <summary>
    /// 只看自己的数据
    /// </summary>
    Self = 0,

    /// <summary>
    /// 本人和本组织
    /// </summary>
    OrgUnit = 1,

    /// <summary>
    /// 本人、本组织及下级
    /// </summary>
    OrgUnitAndBelow = 2,

    /// <summary>
    /// 全部
    /// </summary>
    All = 3,
}

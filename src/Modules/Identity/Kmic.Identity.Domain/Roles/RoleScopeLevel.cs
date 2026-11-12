using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Roles;

/// <summary>
/// 角色上配置的数据范围等级
/// </summary>
/// <remarks>
/// 与框架里访问上下文用的 ScopeLevel 一一对应，编码就是它的成员名；领域层不引用应用层，所以单独定义，由基础设施层转换
/// </remarks>
public sealed class RoleScopeLevel : Enumeration<RoleScopeLevel>
{
    /// <summary>
    /// 只看自己的数据
    /// </summary>
    public static readonly RoleScopeLevel Self = new("Self", "仅本人");

    /// <summary>
    /// 本人和本组织
    /// </summary>
    public static readonly RoleScopeLevel OrgUnit = new("OrgUnit", "本组织");

    /// <summary>
    /// 本人、本组织及下级
    /// </summary>
    public static readonly RoleScopeLevel OrgUnitAndBelow = new("OrgUnitAndBelow", "本组织及下级");

    /// <summary>
    /// 全部
    /// </summary>
    /// <remarks>
    /// 与基类里“全部取值”的静态属性同名，这里按等级的含义取名，所以用 new 隐藏它；取全部等级用 Enumeration&lt;RoleScopeLevel&gt;.All
    /// </remarks>
    public static new readonly RoleScopeLevel All = new("All", "全部");

    private RoleScopeLevel(string code, string name)
        : base(code, name)
    {
    }
}

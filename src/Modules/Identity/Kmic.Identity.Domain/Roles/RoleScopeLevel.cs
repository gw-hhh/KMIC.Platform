using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Roles;

/// <summary>
/// 角色上配置的数据范围等级
/// </summary>
/// <remarks>
/// 编码与框架的 ScopeLevel 成员名一致，由基础设施层转换
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
    /// 此成员表示“全部数据”，隐藏了基类同名属性；遍历等级用 Enumeration&lt;RoleScopeLevel&gt;.All
    /// </remarks>
    public static new readonly RoleScopeLevel All = new("All", "全部");

    private RoleScopeLevel(string code, string name)
        : base(code, name)
    {
    }
}

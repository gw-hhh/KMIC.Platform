using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Roles;

/// <summary>角色数据范围等级，编码与框架 ScopeLevel 成员名一致。</summary>
public sealed class RoleScopeLevel : Enumeration<RoleScopeLevel>
{
    public static readonly RoleScopeLevel Self = new("Self", "仅本人");

    public static readonly RoleScopeLevel OrgUnit = new("OrgUnit", "本组织");

    public static readonly RoleScopeLevel OrgUnitAndBelow = new("OrgUnitAndBelow", "本组织及下级");

    /// <summary>全部数据；隐藏基类 All，遍历全部枚举值请使用 Enumeration&lt;RoleScopeLevel&gt;.All。</summary>
    public static new readonly RoleScopeLevel All = new("All", "全部");

    private RoleScopeLevel(string code, string name)
        : base(code, name)
    {
    }
}

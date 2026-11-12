using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Organization;

/// <summary>
/// 组织单元的状态
/// </summary>
public sealed class OrgUnitStatus : Enumeration<OrgUnitStatus>
{
    /// <summary>
    /// 启用
    /// </summary>
    public static readonly OrgUnitStatus Active = new("Active", "启用");

    /// <summary>
    /// 已停用：不能再分配用户、不能作为锚点，下面不能有启用的下级或用户
    /// </summary>
    public static readonly OrgUnitStatus Disabled = new("Disabled", "已停用");

    private OrgUnitStatus(string code, string name)
        : base(code, name)
    {
    }
}

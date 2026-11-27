using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Organization;

public sealed class OrgUnitStatus : Enumeration<OrgUnitStatus>
{
    public static readonly OrgUnitStatus Active = new("Active", "启用");

    /// <summary>不可新分配用户或设为锚点；停用前须无启用的下级和所属用户。</summary>
    public static readonly OrgUnitStatus Disabled = new("Disabled", "已停用");

    private OrgUnitStatus(string code, string name)
        : base(code, name)
    {
    }
}

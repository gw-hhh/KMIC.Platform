using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Domain.Users;

/// <summary>角色授予的组织锚点，用于计算“本组织”和“本组织及下级”的数据范围。</summary>
public sealed class GrantAnchor : Entity<GrantAnchorId>
{
    private GrantAnchor(GrantAnchorId id, OrgUnitId orgUnitId)
        : base(id) => OrgUnitId = orgUnitId;

    /// <summary>
    /// 起点组织单元
    /// </summary>
    public OrgUnitId OrgUnitId { get; private set; }

    internal static GrantAnchor Create(OrgUnitId orgUnitId) => new(GrantAnchorId.New(), orgUnitId);
}

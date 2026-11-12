using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 授予上的锚点：作为“本组织及下级”数据范围的计算起点的组织单元
/// </summary>
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

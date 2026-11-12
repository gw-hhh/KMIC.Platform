using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 用户拥有某个角色
/// </summary>
/// <remarks>
/// 聚合内的子实体：只能通过 User 创建和修改。同一用户同一角色只能有一条
/// </remarks>
public sealed class UserRoleGrant : Entity<GrantId>
{
    private readonly List<GrantAnchor> _anchors = [];

    private UserRoleGrant(GrantId id, RoleId roleId, UserId grantedBy, DateTimeOffset grantedAt)
        : base(id)
    {
        RoleId = roleId;
        GrantedBy = grantedBy;
        GrantedAt = grantedAt;
    }

    /// <summary>
    /// 角色
    /// </summary>
    public RoleId RoleId { get; private set; }

    /// <summary>
    /// 授予人
    /// </summary>
    public UserId GrantedBy { get; private set; }

    /// <summary>
    /// 授予时间
    /// </summary>
    public DateTimeOffset GrantedAt { get; private set; }

    /// <summary>
    /// 锚点；没有锚点时，“本组织”“本组织及下级”从用户所属组织算起
    /// </summary>
    public IReadOnlyList<GrantAnchor> Anchors => _anchors;

    internal static UserRoleGrant Create(RoleId roleId, UserId grantedBy, DateTimeOffset grantedAt, IEnumerable<OrgUnitId> anchors)
    {
        var grant = new UserRoleGrant(GrantId.New(), roleId, grantedBy, grantedAt);
        grant.SetAnchors(anchors);
        return grant;
    }

    internal void SetAnchors(IEnumerable<OrgUnitId> anchors)
    {
        var wanted = anchors.Distinct().ToList();
        _anchors.RemoveAll(a => !wanted.Contains(a.OrgUnitId));
        foreach (var orgUnitId in wanted.Where(w => _anchors.TrueForAll(a => a.OrgUnitId != w)))
        {
            _anchors.Add(GrantAnchor.Create(orgUnitId));
        }
    }
}

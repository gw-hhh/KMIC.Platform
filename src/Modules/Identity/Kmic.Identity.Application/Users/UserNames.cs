using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 列表和详情里显示的组织单元名、角色名：先分页或读出主体，再按 ID 批量取名字，避免把集合塞进分页的排序查询
/// </summary>
internal static class UserNames
{
    public static async Task<IReadOnlyDictionary<OrgUnitId, string>> OrgUnitsAsync(IIdentityQueryDb db, IEnumerable<OrgUnitId> ids, CancellationToken ct)
    {
        List<OrgUnitId> distinct = [.. ids.Distinct()];
        if (distinct.Count == 0)
        {
            return new Dictionary<OrgUnitId, string>();
        }

        var rows = await db.OrgTrees.SelectMany(t => t.Units).Where(u => distinct.Contains(u.Id)).Select(u => new { u.Id, u.Name }).ToListAsync(ct);
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public static async Task<IReadOnlyDictionary<RoleId, RoleName>> RolesAsync(IIdentityQueryDb db, IEnumerable<RoleId> ids, CancellationToken ct)
    {
        List<RoleId> distinct = [.. ids.Distinct()];
        if (distinct.Count == 0)
        {
            return new Dictionary<RoleId, RoleName>();
        }

        var rows = await db.Roles.Where(r => distinct.Contains(r.Id)).Select(r => new { r.Id, r.Name, r.Status, r.IsProtected }).ToListAsync(ct);
        return rows.ToDictionary(r => r.Id, r => new RoleName(r.Name, r.Status == RoleStatus.Active, r.IsProtected));
    }

    public static async Task<IReadOnlyDictionary<UserId, string>> UsersAsync(IIdentityQueryDb db, IEnumerable<UserId> ids, CancellationToken ct)
    {
        List<UserId> distinct = [.. ids.Distinct()];
        if (distinct.Count == 0)
        {
            return new Dictionary<UserId, string>();
        }

        var rows = await db.Users.Where(u => distinct.Contains(u.Id)).Select(u => new { u.Id, u.DisplayName }).ToListAsync(ct);
        return rows.ToDictionary(r => r.Id, r => r.DisplayName);
    }
}

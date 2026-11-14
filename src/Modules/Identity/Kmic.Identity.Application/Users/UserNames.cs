using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 在主体查询或分页后批量解析用户、角色及组织名称，避免集合关联影响分页排序
/// </summary>
internal static class UserNames
{
    public static async Task<IReadOnlyDictionary<OrgUnitId, string>> OrgUnitsAsync(IIdentityReads db, IEnumerable<OrgUnitId> ids, CancellationToken ct)
    {
        List<OrgUnitId> distinct = [.. ids.Distinct()];
        if (distinct.Count == 0)
        {
            return new Dictionary<OrgUnitId, string>();
        }

        var rows = await db.OrgTrees.SelectMany(t => t.Units).Where(u => distinct.Contains(u.Id)).Select(u => new { u.Id, u.Name }).ToListAsync(ct);
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public static async Task<IReadOnlyDictionary<RoleId, RoleName>> RolesAsync(IIdentityReads db, IEnumerable<RoleId> ids, CancellationToken ct)
    {
        List<RoleId> distinct = [.. ids.Distinct()];
        if (distinct.Count == 0)
        {
            return new Dictionary<RoleId, RoleName>();
        }

        var rows = await db.Roles.Where(r => distinct.Contains(r.Id)).Select(r => new { r.Id, r.Name, r.Status, r.IsProtected }).ToListAsync(ct);
        return rows.ToDictionary(r => r.Id, r => new RoleName(r.Name, r.Status == RoleStatus.Active, r.IsProtected));
    }

    public static async Task<IReadOnlyDictionary<UserId, string>> UsersAsync(IIdentityReads db, IEnumerable<UserId> ids, CancellationToken ct)
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

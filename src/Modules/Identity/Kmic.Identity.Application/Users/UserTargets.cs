using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Users;

// 组装用户管理策略所需的保护角色与最后持有人事实
internal static class UserTargets
{
    public static async Task<UserTarget> BuildAsync(IIdentityReads db, User user, IAccessContext access, CancellationToken ct) =>
        await BuildAsync(db, user, [.. user.Grants.Select(g => g.RoleId)], access, ct);

    public static async Task<UserTarget> BuildAsync(IIdentityReads db, User user, IReadOnlyList<RoleId> roleIds, IAccessContext access, CancellationToken ct)
    {
        var operatorProtected = await OperatorFacts.IsProtectedAsync(db, access, ct);
        if (roleIds.Count == 0)
        {
            return new UserTarget(user, operatorProtected, HoldsProtectedRole: false, IsLastProtectedHolder: false);
        }

        var holdsProtected = await db.Roles.AnyAsync(r => r.IsProtected && roleIds.Contains(r.Id), ct);
        var isEnabledHolder = user.CanLogin && await db.Roles.AnyAsync(r => r.IsProtected && r.Status == RoleStatus.Active && roleIds.Contains(r.Id), ct);
        var isLast = isEnabledHolder && !await ProtectedHolders.EnabledHolders(db.Users, db.Roles).AnyAsync(u => u.Id != user.Id, ct);
        return new UserTarget(user, operatorProtected, holdsProtected, isLast);
    }
}

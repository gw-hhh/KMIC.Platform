using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 为一个用户算出交给资源策略的事实
/// </summary>
internal static class UserTargets
{
    /// <summary>
    /// 用命令里加载的用户（带着授予）算
    /// </summary>
    public static async Task<UserTarget> BuildAsync(IIdentityQueryDb db, User user, IAccessContext access, CancellationToken ct) =>
        await BuildAsync(db, user, [.. user.Grants.Select(g => g.RoleId)], access, ct);

    /// <summary>
    /// 查询里读出来的用户不带授予，由调用方把他拥有的角色一起给进来
    /// </summary>
    public static async Task<UserTarget> BuildAsync(IIdentityQueryDb db, User user, IReadOnlyList<RoleId> roleIds, IAccessContext access, CancellationToken ct)
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

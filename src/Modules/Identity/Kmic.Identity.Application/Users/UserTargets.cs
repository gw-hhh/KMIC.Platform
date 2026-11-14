using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 构建用户管理策略所需的授权事实
/// </summary>
internal static class UserTargets
{
    /// <summary>
    /// 根据命令加载的用户及其授予构建策略目标
    /// </summary>
    public static async Task<UserTarget> BuildAsync(IIdentityReads db, User user, IAccessContext access, CancellationToken ct) =>
        await BuildAsync(db, user, [.. user.Grants.Select(g => g.RoleId)], access, ct);

    /// <summary>
    /// 根据查询取得的用户和调用方提供的角色 ID 构建策略目标
    /// </summary>
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

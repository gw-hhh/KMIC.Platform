using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Users.Grants;

// 授予类命令共用的加锁加载与资源策略鉴权
internal static class GrantLoading
{
    public static async Task<Result<GrantContext>> LoadForAsync(
        IUserRepository users,
        IIdentityReads db,
        IResourceAuthorizer<GrantTarget, GrantAction> authorizer,
        UserId userId,
        long expectedVersion,
        RoleId roleId,
        GrantAction action,
        IAccessContext access,
        CancellationToken ct)
    {
        // 收回前加串行锁，防止并发移除最后一位可登录的受保护角色持有人
        if (action == GrantAction.Revoke)
        {
            await users.LockProtectedHolderChecksAsync(ct);
        }

        var loaded = await users.GetForUpdateAsync(userId, expectedVersion, ct);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var role = await db.Roles.Where(r => r.Id == roleId).Select(r => new { r.IsProtected, r.Status }).SingleOrDefaultAsync(ct);
        if (role is null)
        {
            return IdentityErrors.RoleNotFound;
        }

        var user = loaded.Value;
        var roleIsActive = role.Status == RoleStatus.Active;
        var wouldRemoveLast = action == GrantAction.Revoke && role.IsProtected && roleIsActive && await RemovesLastHolderAsync(db, user, roleId, ct);
        var target = new GrantTarget(user, roleId, role.IsProtected, roleIsActive, await OperatorFacts.IsProtectedAsync(db, access, ct), wouldRemoveLast);
        var allowed = authorizer.Check(target, action);
        return allowed.IsFailure ? allowed.Error : new GrantContext(user, roleIsActive);
    }

    // 仅当用户可登录且收回后全库再无其他可登录的受保护持有人时返回 true
    private static async Task<bool> RemovesLastHolderAsync(IIdentityReads db, User user, RoleId revoked, CancellationToken ct)
    {
        if (!user.CanLogin)
        {
            return false;
        }

        List<RoleId> remaining = [.. user.Grants.Select(g => g.RoleId).Where(id => id != revoked)];
        var stillHolder = remaining.Count > 0 && await db.Roles.AnyAsync(r => r.IsProtected && r.Status == RoleStatus.Active && remaining.Contains(r.Id), ct);
        return !stillHolder && !await ProtectedHolders.EnabledHolders(db.Users, db.Roles).AnyAsync(u => u.Id != user.Id, ct);
    }

    public static IReadOnlyDictionary<string, string> RoleDetails(RoleId roleId) =>
        new Dictionary<string, string>(StringComparer.Ordinal) { ["roleId"] = roleId.Value.ToString() };
}

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

/// <summary>
/// 授予类命令共用的加载与授权检查
/// </summary>
internal static class GrantLoading
{
    public static async Task<Result<GrantContext>> LoadForAsync(
        IUserRepository users,
        IIdentityQueryDb db,
        IResourceAuthorizer<GrantTarget, GrantAction> authorizer,
        UserId userId,
        long expectedVersion,
        RoleId roleId,
        GrantAction action,
        IAccessContext access,
        CancellationToken ct)
    {
        // 收回要判断“是不是最后一个管理员”，先排队再读
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

    // 收回这个角色之后：他自己不再是启用的受保护角色持有人（没有别的受保护角色顶着），而且没有别人
    private static async Task<bool> RemovesLastHolderAsync(IIdentityQueryDb db, User user, RoleId revoked, CancellationToken ct)
    {
        if (!user.CanLogin)
        {
            return false;
        }

        List<RoleId> remaining = [.. user.Grants.Select(g => g.RoleId).Where(id => id != revoked)];
        var stillHolder = remaining.Count > 0 && await db.Roles.AnyAsync(r => r.IsProtected && r.Status == RoleStatus.Active && remaining.Contains(r.Id), ct);
        return !stillHolder && !await ProtectedHolders.EnabledHolders(db.Users, db.Roles).AnyAsync(u => u.Id != user.Id, ct);
    }

    /// <summary>
    /// 安全事件里记下是哪个角色
    /// </summary>
    public static IReadOnlyDictionary<string, string> RoleDetails(RoleId roleId) =>
        new Dictionary<string, string>(StringComparer.Ordinal) { ["roleId"] = roleId.Value.ToString() };
}

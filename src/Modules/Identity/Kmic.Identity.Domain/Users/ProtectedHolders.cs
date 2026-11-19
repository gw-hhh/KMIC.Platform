using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 受保护角色持有人检查
/// </summary>
public static class ProtectedHolders
{
    /// <summary>
    /// 持有已启用受保护角色的启用普通用户，读写两侧共用这个判定
    /// </summary>
    public static IQueryable<User> EnabledHolders(IQueryable<User> users, IQueryable<Role> roles)
    {
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(roles);

        return users.Where(u => u.Kind == UserKind.Normal
            && u.Status == UserStatus.Active
            && u.Grants.Any(g => roles.Any(r => r.Id == g.RoleId && r.IsProtected && r.Status == RoleStatus.Active)));
    }
}

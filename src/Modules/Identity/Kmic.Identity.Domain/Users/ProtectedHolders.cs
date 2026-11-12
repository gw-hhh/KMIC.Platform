using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 受保护角色的启用持有人：系统里必须至少保留一个，否则没人能管理授权
/// </summary>
public static class ProtectedHolders
{
    /// <summary>
    /// 启用的普通用户里，拥有启用的受保护角色的那些
    /// </summary>
    /// <remarks>
    /// 命令里的检查用写库的集合，查询里的展示用只读的集合，两处用同一个条件，所以放在这里。
    /// 角色已经停用的授予不算：持有一个停用的受保护角色，登录后得不到任何权限
    /// </remarks>
    /// <param name="users">用户集合</param>
    /// <param name="roles">角色集合</param>
    public static IQueryable<User> EnabledHolders(IQueryable<User> users, IQueryable<Role> roles)
    {
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(roles);

        return users.Where(u => u.Kind == UserKind.Normal
            && u.Status == UserStatus.Active
            && u.Grants.Any(g => roles.Any(r => r.Id == g.RoleId && r.IsProtected && r.Status == RoleStatus.Active)));
    }
}

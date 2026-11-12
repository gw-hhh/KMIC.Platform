using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Grants;

/// <summary>
/// 授权检查通过后交给命令的上下文：要修改的用户，加上角色当前是否启用
/// </summary>
/// <param name="User">要修改的用户</param>
/// <param name="RoleIsActive">角色是否启用；授予时角色必须启用</param>
internal sealed record GrantContext(User User, bool RoleIsActive);

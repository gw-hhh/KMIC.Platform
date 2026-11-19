using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Grants;

/// <summary>
/// 授权通过后的角色授予上下文，包含目标用户及角色启用状态
/// </summary>
/// <param name="User">要修改的用户</param>
/// <param name="RoleIsActive">角色是否启用</param>
internal sealed record GrantContext(User User, bool RoleIsActive);

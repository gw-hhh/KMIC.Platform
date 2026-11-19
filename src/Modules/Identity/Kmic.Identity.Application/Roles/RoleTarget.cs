using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles;

/// <summary>
/// 角色管理策略的目标，操作者当前角色是否受保护由处理器查好传入
/// </summary>
/// <param name="Role">被管理的角色</param>
/// <param name="OperatorIsProtected">操作者当前角色是否受保护</param>
public sealed record RoleTarget(Role Role, bool OperatorIsProtected);

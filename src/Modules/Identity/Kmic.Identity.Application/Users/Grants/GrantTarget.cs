using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Grants;

/// <summary>角色授予策略的评估目标。</summary>
/// <param name="User">被授予的用户</param>
/// <param name="RoleId">角色 ID</param>
/// <param name="RoleIsProtected">角色是否受保护</param>
/// <param name="RoleIsActive">角色是否启用</param>
/// <param name="OperatorIsProtected">操作者当前角色是否受保护</param>
/// <param name="WouldRemoveLastProtectedHolder">是否移除最后持有人，即收回后不再存在可登录且持有已启用受保护角色的用户</param>
public sealed record GrantTarget(User User, RoleId RoleId, bool RoleIsProtected, bool RoleIsActive, bool OperatorIsProtected, bool WouldRemoveLastProtectedHolder);

using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Grants;

/// <summary>
/// 管理角色授予时交给资源策略的对象：被授予的用户和角色，加上聚合内判断不了的事实
/// </summary>
/// <param name="User">被授予的用户</param>
/// <param name="RoleId">角色</param>
/// <param name="RoleIsProtected">角色是否受保护</param>
/// <param name="RoleIsActive">角色是否启用</param>
/// <param name="OperatorIsProtected">操作者当前的角色是不是受保护角色</param>
/// <param name="WouldRemoveLastProtectedHolder">收回这个角色之后，系统里是否一个启用的受保护角色持有人都不剩</param>
public sealed record GrantTarget(User User, RoleId RoleId, bool RoleIsProtected, bool RoleIsActive, bool OperatorIsProtected, bool WouldRemoveLastProtectedHolder);

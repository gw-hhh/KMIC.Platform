using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Grants;

/// <summary>
/// 角色授予的资源策略（D11）：堵住“自我提权到系统管理员”
/// </summary>
/// <remarks>
/// 不能改自己的授予；受保护角色只有当前角色也是受保护角色的人才能授予、收回、改锚点；
/// 不能收回最后一个启用的受保护角色持有人的受保护角色。其余角色由用户管理员自行管理
/// </remarks>
public sealed class GrantPolicy : IResourcePolicy<GrantTarget, GrantAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, GrantTarget resource, GrantAction action)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);

        if (resource.User.Kind == UserKind.System)
        {
            return PolicyDecision.Deny(DenialReason.ResourceDenied, IdentityErrors.SystemUserImmutable);
        }

        if (resource.User.Id == access.UserId)
        {
            return PolicyDecision.Deny(DenialReason.ResourceDenied, IdentityErrors.CannotChangeOwnGrants);
        }

        if (resource.RoleIsProtected && !resource.OperatorIsProtected)
        {
            return PolicyDecision.Deny(DenialReason.ResourceDenied, IdentityErrors.ProtectedRoleChangeDenied);
        }

        if (action == GrantAction.Revoke && resource.WouldRemoveLastProtectedHolder)
        {
            return PolicyDecision.Deny(DenialReason.ResourceDenied, IdentityErrors.LastProtectedHolder);
        }

        return PolicyDecision.Allow;
    }
}

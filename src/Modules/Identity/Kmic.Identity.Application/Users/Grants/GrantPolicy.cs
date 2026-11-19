using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Grants;

/// <summary>
/// 角色授予的操作限制
/// </summary>
/// <remarks>
/// 收回授予后，系统里必须仍有可登录、持有已启用受保护角色的用户
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

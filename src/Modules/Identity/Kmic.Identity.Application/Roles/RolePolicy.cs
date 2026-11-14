using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Identity.Domain.Errors;

namespace Kmic.Identity.Application.Roles;

/// <summary>
/// 受保护角色的管理限制：操作者的当前角色也必须受保护
/// </summary>
public sealed class RolePolicy : IResourcePolicy<RoleTarget, RoleAdminAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, RoleTarget resource, RoleAdminAction action)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);

        return resource.Role.IsProtected && !resource.OperatorIsProtected
            ? PolicyDecision.Deny(DenialReason.ResourceDenied, IdentityErrors.ProtectedRoleChangeDenied)
            : PolicyDecision.Allow;
    }
}

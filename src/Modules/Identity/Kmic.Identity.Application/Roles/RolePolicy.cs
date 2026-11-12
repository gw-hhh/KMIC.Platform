using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Identity.Domain.Errors;

namespace Kmic.Identity.Application.Roles;

/// <summary>
/// 角色管理的资源策略：受保护角色只有当前角色也是受保护角色的人才能管
/// </summary>
/// <remarks>
/// 堵住“自我提权到系统管理员”这条最主要的路径；其余角色由角色管理员管理，不受限
/// </remarks>
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

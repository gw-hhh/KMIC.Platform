using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 用户管理的资源策略
/// </summary>
/// <remarks>
/// 三条线：系统账号谁都不能动；不能对自己做会把自己踢出去的事；对持有受保护角色的人，凭据类动作只有同样持有受保护角色的人
/// 才能做，并且不能让系统里一个启用的受保护角色持有人都不剩。这样“重置管理员的密码再登录”这条提权的近路被堵上
/// </remarks>
public sealed class UserPolicy : IResourcePolicy<UserTarget, UserAdminAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, UserTarget resource, UserAdminAction action)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);

        if (resource.User.Kind == UserKind.System)
        {
            return PolicyDecision.Deny(DenialReason.ResourceDenied, IdentityErrors.SystemUserImmutable);
        }

        if (resource.User.Id == access.UserId && (action == UserAdminAction.Disable || action == UserAdminAction.ForceLogout))
        {
            return PolicyDecision.Deny(DenialReason.ResourceDenied, IdentityErrors.CannotActOnSelf);
        }

        if (resource.HoldsProtectedRole && !resource.OperatorIsProtected && IsCredentialAction(action))
        {
            return PolicyDecision.Deny(DenialReason.ResourceDenied, IdentityErrors.ProtectedRoleChangeDenied);
        }

        if (resource.IsLastProtectedHolder && action == UserAdminAction.Disable)
        {
            return PolicyDecision.Deny(DenialReason.ResourceDenied, IdentityErrors.LastProtectedHolder);
        }

        return PolicyDecision.Allow;
    }

    private static bool IsCredentialAction(UserAdminAction action) =>
        action == UserAdminAction.ResetPassword || action == UserAdminAction.ChangeCredentials || action == UserAdminAction.ForceLogout;
}

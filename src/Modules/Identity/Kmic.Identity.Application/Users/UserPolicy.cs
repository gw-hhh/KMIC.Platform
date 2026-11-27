using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users;

/// <summary>用户管理的操作限制，禁止禁用最后一位可登录的受保护角色持有人。</summary>
public sealed class UserPolicy : IResourcePolicy<UserTarget, UserAdminAction>
{
    public PolicyDecision Evaluate(IAccessContext access, UserTarget resource, UserAdminAction action)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);

        // 禁止模拟系统账号、本人及受保护角色持有人，防止借模拟身份绕过凭据保护
        if (action == UserAdminAction.Impersonate || action == UserAdminAction.ImpersonateWrite)
        {
            return resource.User.Kind == UserKind.System || resource.User.Id == access.UserId || resource.HoldsProtectedRole
                ? PolicyDecision.Deny(DenialReason.ResourceDenied, IdentityErrors.ImpersonationDenied)
                : PolicyDecision.Allow;
        }

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

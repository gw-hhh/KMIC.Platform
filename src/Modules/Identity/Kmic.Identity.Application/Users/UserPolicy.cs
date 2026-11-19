using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 用户管理的操作限制
/// </summary>
/// <remarks>
/// 不能禁用最后一个能登录的受保护角色持有人
/// </remarks>
public sealed class UserPolicy : IResourcePolicy<UserTarget, UserAdminAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, UserTarget resource, UserAdminAction action)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);

        // 禁止模拟自己、系统账号及受保护角色持有人，避免通过模拟身份绕过管理员凭据保护
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

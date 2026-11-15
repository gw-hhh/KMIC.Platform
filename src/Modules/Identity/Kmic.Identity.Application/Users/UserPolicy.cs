using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 用户管理的操作限制
/// </summary>
/// <remarks>
/// 系统账号不可修改，不能禁用自己或强制自己下线。
/// 目标用户持有受保护角色时，重置密码、修改账号或手机号、强制下线均要求操作者的当前角色受保护。
/// 不能禁用最后一位可登录且持有已启用受保护角色的用户
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

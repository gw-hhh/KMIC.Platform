using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 用户管理的资源动作目录：详情响应里的“可用动作”据此计算
/// </summary>
public sealed class UserActions : ResourceActionCatalog<UserTarget, UserAdminAction>
{
    /// <inheritdoc />
    protected override void Define(IResourceActionBuilder<UserTarget, UserAdminAction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Action(UserAdminAction.Edit, IdentityPermissions.UserManage);
        builder.Action(UserAdminAction.ChangeCredentials, IdentityPermissions.UserManage);
        builder.Action(UserAdminAction.Disable, IdentityPermissions.UserManage, state: IsActive);
        builder.Action(UserAdminAction.Enable, IdentityPermissions.UserManage, state: IsDisabled);
        builder.Action(UserAdminAction.ResetPassword, IdentityPermissions.UserResetPassword);
        builder.Action(UserAdminAction.Unlock, IdentityPermissions.UserUnlock);
        builder.Action(UserAdminAction.ForceLogout, IdentityPermissions.UserForceLogout);
        builder.Action(UserAdminAction.Impersonate, IdentityPermissions.UserImpersonate, state: IsActive);
        builder.Action(UserAdminAction.ImpersonateWrite, IdentityPermissions.UserImpersonateWrite, state: IsActive);
    }

    private static Result IsActive(UserTarget target) => target.User.Status == UserStatus.Active ? Result.Success() : IdentityErrors.InvalidState;

    private static Result IsDisabled(UserTarget target) => target.User.Status == UserStatus.Disabled ? Result.Success() : IdentityErrors.InvalidState;
}

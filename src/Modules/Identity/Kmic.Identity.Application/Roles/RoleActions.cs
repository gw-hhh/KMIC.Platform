using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;

namespace Kmic.Identity.Application.Roles;

/// <summary>
/// 角色管理操作目录，用于计算详情中的可用操作
/// </summary>
public sealed class RoleActions : ResourceActionCatalog<RoleTarget, RoleAdminAction>
{
    /// <inheritdoc />
    protected override void Define(IResourceActionBuilder<RoleTarget, RoleAdminAction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Action(RoleAdminAction.Edit, IdentityPermissions.RoleManage);
        builder.Action(RoleAdminAction.SetPermissions, IdentityPermissions.RoleManage, state: NotBuiltIn);
        builder.Action(RoleAdminAction.SetDataScopes, IdentityPermissions.RoleManage, state: NotBuiltIn);
        builder.Action(RoleAdminAction.Disable, IdentityPermissions.RoleManage, state: NotBuiltIn);
        builder.Action(RoleAdminAction.Enable, IdentityPermissions.RoleManage);
    }

    // 内置的系统管理员角色：权限点、数据范围不能手工改，也不能停用
    private static Result NotBuiltIn(RoleTarget target) => target.Role.IsBuiltIn ? IdentityErrors.SystemRoleImmutable : Result.Success();
}

using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles;

/// <summary>
/// 角色管理命令共用的加载与授权检查
/// </summary>
internal static class RoleAdminLoading
{
    /// <summary>
    /// 加载要修改的角色（带版本检查），并按资源策略检查操作者能否做这个动作
    /// </summary>
    public static async Task<Result<Role>> LoadForAsync(
        IRoleRepository roles,
        IIdentityQueryDb db,
        IResourceAuthorizer<RoleTarget, RoleAdminAction> authorizer,
        RoleId id,
        long expectedVersion,
        RoleAdminAction action,
        IAccessContext access,
        CancellationToken ct)
    {
        var loaded = await roles.GetForUpdateAsync(id, expectedVersion, ct);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var allowed = authorizer.Check(new RoleTarget(loaded.Value, await OperatorFacts.IsProtectedAsync(db, access, ct)), action);
        return allowed.IsFailure ? allowed.Error : loaded.Value;
    }
}

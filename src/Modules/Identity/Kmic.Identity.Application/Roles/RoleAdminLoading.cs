using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles;

// 角色管理命令共用的乐观锁加载与资源策略鉴权
internal static class RoleAdminLoading
{
    public static async Task<Result<Role>> LoadForAsync(
        IRoleRepository roles,
        IIdentityReads db,
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

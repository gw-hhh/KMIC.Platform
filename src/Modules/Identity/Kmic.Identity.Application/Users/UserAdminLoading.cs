using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users;

// 用户管理命令共用的加锁加载与资源策略鉴权（指定版本号时校验乐观锁）
internal static class UserAdminLoading
{
    public static async Task<Result<User>> LoadForAsync(
        IUserRepository users,
        IIdentityReads db,
        IResourceAuthorizer<UserTarget, UserAdminAction> authorizer,
        UserId id,
        long? expectedVersion,
        UserAdminAction action,
        IAccessContext access,
        CancellationToken ct)
    {
        // 禁用前加串行锁，防止并发禁用最后一位可登录的受保护角色持有人
        if (action == UserAdminAction.Disable)
        {
            await users.LockProtectedHolderChecksAsync(ct);
        }

        var loaded = await users.GetForUpdateAsync(id, expectedVersion, ct);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var allowed = authorizer.Check(await UserTargets.BuildAsync(db, loaded.Value, access, ct), action);
        return allowed.IsFailure ? allowed.Error : loaded.Value;
    }
}

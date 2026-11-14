using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 用户管理命令共用的加载与授权检查
/// </summary>
internal static class UserAdminLoading
{
    /// <summary>
    /// 加载目标用户并按资源策略授权，指定版本号时校验版本
    /// </summary>
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
        // 禁用前串行检查受保护角色持有人，避免并发禁用后失去最后一位可登录的持有人
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
